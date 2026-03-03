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
      var banco = ExtrairBanco(sqlBruto, schemas);
      var sql = ExtrairSql(sqlBruto);

      _logger.LogInformation($"SQL gerado: {sql}");

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
        Você é um gerador de SQL puro para PostgreSQL. Sua única saída deve ser exatamente neste formato:
        
        BANCO: IA
        SQL: <comando SQL válido>
        
        REGRAS ESTRITAS:
        - Responda APENAS no formato acima. Nada mais.
        - Sem explicações, sem markdown, sem comentários
        - Todas as tabelas ficam em schemas com sufixo _fdw. Exemplo: se o banco é "Usuarios" e a tabela é "users", use "usuarios_fdw.users"
        - Para montar o schema use: <nome_do_banco_em_minusculo>_fdw.<nome_da_tabela>
        - Quando a pergunta envolver dados de múltiplas tabelas ou bancos, use JOIN
        - Identifique as relações entre tabelas pelos nomes das colunas (ex: usuario_id, pedido_id)
        - Nunca selecione colunas sensíveis como senha_hash, refresh_token, refresh_token_expiry ou similares
        - Use sintaxe PostgreSQL pura. Nunca use parâmetros como :param ou ?. Se precisar filtrar por um valor específico que não foi fornecido, omita o WHERE.
        - NUNCA filtre por colunas sensíveis no WHERE
        - Para excluir colunas sensíveis simplesmente não as inclua no SELECT
        - Nunca adicione filtros WHERE que não foram explicitamente solicitados pelo usuário

        Schemas disponíveis:
        {contexto}
        
        Pergunta do usuário: {pergunta}
       """;
    }

    private static string MontarPromptResposta(string pergunta, string sql, List<Dictionary<string, object?>> dados)
    {
      var dadosJson = JsonSerializer.Serialize(dados, new JsonSerializerOptions
      {
        WriteIndented = true
      });

      return $"""
        Responda a pergunta abaixo em português de forma direta e objetiva em no máximo 3 frases.
        Não mencione SQL, banco de dados ou detalhes técnicos.
        Baseie sua resposta nos dados fornecidos.
        
        Pergunta: {pergunta}
        Dados retornados: {dadosJson}
        
        Resposta direta:
       """;
    }

    private static string ExtrairSql(string sqlBruto)
    {
      // remove linha do BANCO: se presente
      var linhas = sqlBruto.Split('\n')
          .Where(l => !l.StartsWith("BANCO:", StringComparison.OrdinalIgnoreCase))
          .ToArray();

      var sql = string.Join("\n", linhas)
          .Replace("SQL:", "", StringComparison.OrdinalIgnoreCase)
          .Replace("```sql", "")
          .Replace("```", "")
          .Trim();

      var semicolon = sql.IndexOf(';');
      if (semicolon > 0)
        sql = sql[..(semicolon + 1)];

      return sql;
    }

    private string DeterminarBanco(List<string> schemas)
    {
      var primeiroBanco = schemas
          .FirstOrDefault(s => s.Contains("Banco:"))
          ?.Split('\n')
          .FirstOrDefault(l => l.StartsWith("Banco:"))?
          .Replace("Banco:", "")
          .Trim();

      if (!string.IsNullOrEmpty(primeiroBanco))
      {
        _logger.LogInformation($"Banco determinado pelo schema mais relevante: {primeiroBanco}");
        return primeiroBanco;
      }

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

    private string ExtrairBanco(string sqlBruto, List<string> schemas)
    {
      var bancosValidos = _configuration
          .GetSection("ConnectionStrings")
          .GetChildren()
          .Select(x => x.Key)
          .Where(k => k != "IA")
          .ToList();

      var linhas = sqlBruto.Split('\n');

      foreach (var linha in linhas)
      {
        if (linha.StartsWith("BANCO:", StringComparison.OrdinalIgnoreCase))
        {
          var banco = linha
              .Replace("BANCO:", "", StringComparison.OrdinalIgnoreCase)
              .Trim();

          if (bancosValidos.Contains(banco, StringComparer.OrdinalIgnoreCase))
          {
            _logger.LogInformation($"Banco extraído da resposta do modelo: {banco}");
            return banco;
          }
        }
      }

      _logger.LogWarning("Modelo não retornou banco no formato esperado, usando contagem");
      return DeterminarBanco(schemas);
    }

    private async Task<List<Dictionary<string, object?>>> ExecutarSqlAsync(string sql, string banco, CancellationToken cancellationToken)
    {
      var connectionString = _configuration.GetConnectionString("IA");
      var resultado = new List<Dictionary<string, object?>>();

      await using var connection = new NpgsqlConnection(connectionString);
      await connection.OpenAsync(cancellationToken);

      await using var command = new NpgsqlCommand(sql, connection);
      await using var reader = await command.ExecuteReaderAsync(cancellationToken);

      while (await reader.ReadAsync(cancellationToken))
      {
        var row = new Dictionary<string, object?>();
        for (var i = 0; i < reader.FieldCount; i++)
          row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);

        resultado.Add(row);
      }

      _logger.LogInformation($"Query retornou {resultado.Count} registros");
      return resultado;
    }
  }
}
