using ECommerce.IA.Api.Data;
using ECommerce.IA.Api.Entities;
using ECommerce.IA.Api.Services.Interfaces;
using Npgsql;
using System.Text;

namespace ECommerce.IA.Api.Services
{
  public sealed class SchemaService : ISchemaService
  {
    private readonly IADbContext _context;
    private readonly IOllamaService _ollamaService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SchemaService> _logger;

    private readonly Dictionary<string, string> _bancos = new()
    {
      { "Usuarios", "Usuarios" },
      { "Produtos", "Produtos" },
      { "Pedidos", "Pedidos" }
    };

    public SchemaService(
      IADbContext context,
      IOllamaService ollamaService,
      IConfiguration configuration,
      ILogger<SchemaService> logger
    )
    {
      _context = context;
      _ollamaService = ollamaService;
      _configuration = configuration;
      _logger = logger;
    }

    public async Task IndexarSchemasAsync(CancellationToken cancellationToken = default)
    {
      _logger.LogInformation("Iniciando indexação dos schemas");

      // limpa embeddings antigos
      _context.SchemaEmbeddings.RemoveRange(_context.SchemaEmbeddings);
      await _context.SaveChangesAsync(cancellationToken);

      foreach (var (connectionKey, bancoNome) in _bancos)
      {
        var connectionString = _configuration.GetConnectionString(connectionKey);

        if (string.IsNullOrEmpty(connectionString))
          continue;

        await IndexarBancoAsync(connectionString, bancoNome, cancellationToken);
      }

      _logger.LogInformation("Indexação concluída");
    }

    private async Task IndexarBancoAsync(string connectionString, string bancoNome, CancellationToken cancellationToken)
    {
      _logger.LogInformation($"Indexando banco {bancoNome}");

      await using var connection = new NpgsqlConnection(connectionString);
      await connection.OpenAsync(cancellationToken);

      var tabelas = await ObterTabelasAsync(connection, cancellationToken);

      foreach (var tabela in tabelas)
      {
        var colunas = await ObterColunasAsync(connection, tabela, cancellationToken);

        var descricao = MontarDescricao(bancoNome, tabela, colunas);

        _logger.LogInformation($"Gerando embedding para {bancoNome}.{tabela}");

        var embedding = await _ollamaService.GerarEmbeddingAsync(descricao, cancellationToken);

        _context.SchemaEmbeddings.Add(new SchemaEmbedding
        {
          Tabela = tabela,
          Banco = bancoNome,
          Descricao = descricao,
          Embedding = embedding
        });
      }

      await _context.SaveChangesAsync(cancellationToken);
    }

    private static async Task<List<string>> ObterTabelasAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
      var tabelas = new List<string>();

      var sql = """
        SELECT table_name 
        FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_type = 'BASE TABLE'
        AND table_name NOT IN ('outbox_messages', '__EFMigrationsHistory')
        ORDER BY table_name
       """;

      await using var command = new NpgsqlCommand(sql, connection);
      await using var reader = await command.ExecuteReaderAsync(cancellationToken);

      while (await reader.ReadAsync(cancellationToken))
        tabelas.Add(reader.GetString(0));

      return tabelas;
    }

    private static async Task<List<(string Nome, string Tipo, bool Nullable)>> ObterColunasAsync(NpgsqlConnection connection, string tabela, CancellationToken cancellationToken)
    {
      var colunas = new List<(string, string, bool)>();

      var sql = """
        SELECT column_name, data_type, is_nullable
        FROM information_schema.columns
        WHERE table_schema = 'public'
        AND table_name = @tabela
        ORDER BY ordinal_position
       """;

      await using var command = new NpgsqlCommand(sql, connection);
      command.Parameters.AddWithValue("tabela", tabela);
      await using var reader = await command.ExecuteReaderAsync(cancellationToken);

      while (await reader.ReadAsync(cancellationToken))
      {
        colunas.Add((
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2) == "YES"
        ));
      }

      return colunas;
    }

    private static string MontarDescricao(string banco, string tabela, List<(string Nome, string Tipo, bool Nullable)> colunas)
    {
      var sb = new StringBuilder();
      sb.AppendLine($"Banco: {banco}");
      sb.AppendLine($"Tabela: {tabela}");
      sb.AppendLine("Colunas:");

      foreach (var (nome, tipo, nullable) in colunas)
      {
        var nullableStr = nullable ? "nullable" : "not null";
        sb.AppendLine($"  - {nome} ({tipo}, {nullableStr})");
      }

      return sb.ToString();
    }
  }
}
