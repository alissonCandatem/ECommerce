using ECommerce.IA.Api.Responses;
using ECommerce.IA.Api.Services.Interfaces;
using Npgsql;
using System.Text.Json;

namespace ECommerce.IA.Api.Services
{
  public sealed class QueryService : IQueryService
  {
    private readonly IEmbeddingService _embeddingService;
    private readonly IOllamaService _ollamaService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<QueryService> _logger;

    public QueryService(
        IEmbeddingService embeddingService,
        IOllamaService ollamaService,
        IConfiguration configuration,
        ILogger<QueryService> logger)
    {
      _embeddingService = embeddingService;
      _ollamaService = ollamaService;
      _configuration = configuration;
      _logger = logger;
    }

    public async Task<ConsultaResponse> ConsultarAsync(string pergunta, CancellationToken cancellationToken = default)
    {
      var schemas = await _embeddingService.BuscarSchemasRelevantesAsync(pergunta, cancellationToken: cancellationToken);

      var contexto = string.Join("\n\n", schemas);

      var promptSql = MontarPromptSql(pergunta, contexto);

      var sqlBruto = await _ollamaService.GerarSqlAsync(promptSql, cancellationToken);
      var sql = ExtrairSql(sqlBruto);

      _logger.LogInformation($"SQL gerado: {sql}");

      var banco = DeterminarBanco(schemas);

      var dados = await ExecutarSqlAsync(sql, banco, cancellationToken);

      var promptResposta = MontarPromptResposta(pergunta, sql, dados);
      var resposta = await _ollamaService.GerarRespostaAsync(promptResposta, cancellationToken);

      return new ConsultaResponse
      {
        Pergunta = pergunta,
        SqlGerado = sql,
        Resposta = resposta,
        Dados = dados
      };
    }

    private static string MontarPromptSql(string pergunta, string contexto)
    {
      return $"""
        Você é um especialista em SQL e PostgreSQL.
                
        Aqui estão os schemas das tabelas relevantes:
                
        {contexto}
                
        Pergunta: {pergunta}
                
        Gere APENAS o SQL necessário para responder a pergunta.
        Não inclua explicações, comentários ou markdown.
        Retorne somente o SQL puro.
        Use apenas as tabelas e colunas presentes nos schemas acima.
       """;
    }

    private static string MontarPromptResposta(string pergunta, string sql, List<Dictionary<string, object?>> dados)
    {
      var dadosJson = JsonSerializer.Serialize(dados, new JsonSerializerOptions
      {
        WriteIndented = true
      });

      return $"""
        Você é um assistente amigável de e-commerce.
                
        O usuário fez a seguinte pergunta: {pergunta}
                
        O SQL executado foi: {sql}
                
        Os dados retornados foram:
        {dadosJson}
                
        Responda a pergunta do usuário de forma clara e amigável em português,
        baseando-se nos dados retornados. Seja direto e objetivo.
       """;
    }

    private static string ExtrairSql(string sqlBruto)
    {
      // remove markdown se o modelo retornou com ```sql
      var sql = sqlBruto.Replace("```sql", "").Replace("```", "").Trim();

      // pega só a primeira query se retornou várias
      var semicolon = sql.IndexOf(';');
      if (semicolon > 0)
        sql = sql[..(semicolon + 1)];

      return sql;
    }

    private string DeterminarBanco(List<string> schemas)
    {
      // verifica qual banco tem mais schemas relevantes
      var contagem = new Dictionary<string, int>
      {
        { "Usuarios", 0 },
        { "Produtos", 0 },
        { "Pedidos", 0 }
      };

      foreach (var schema in schemas)
      {
        foreach (var banco in contagem.Keys)
        {
          if (schema.Contains($"Banco: {banco}"))
            contagem[banco]++;
        }
      }

      return contagem.MaxBy(x => x.Value).Key;
    }

    private async Task<List<Dictionary<string, object?>>> ExecutarSqlAsync(string sql, string banco, CancellationToken cancellationToken)
    {
      var connectionString = _configuration.GetConnectionString(banco);
      var resultado = new List<Dictionary<string, object?>>();

      await using var connection = new NpgsqlConnection(connectionString);
      await connection.OpenAsync(cancellationToken);

      await using var command = new NpgsqlCommand(sql, connection);
      await using var reader = await command.ExecuteReaderAsync(cancellationToken);

      while (await reader.ReadAsync(cancellationToken))
      {
        var row = new Dictionary<string, object?>();
        for (var i = 0; i < reader.FieldCount; i++)
        {
          row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        }

        resultado.Add(row);
      }

      _logger.LogInformation($"Query retornou {resultado.Count} registros");

      return resultado;
    }
  }
}
