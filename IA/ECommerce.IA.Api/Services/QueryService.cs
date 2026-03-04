using ECommerce.IA.Api.Responses;
using ECommerce.IA.Api.Services.Interfaces;

namespace ECommerce.IA.Api.Services
{
  public sealed class QueryService : IQueryService
  {
    private readonly IQueryCacheService _queryCacheService;
    private readonly IEmbeddingService _embeddingService;
    private readonly ISqlGeneratorService _sqlGenerator;
    private readonly ISqlExecutorService _sqlExecutor;
    private readonly IResponseFormatterService _responseFormatter;
    private readonly ILogger<QueryService> _logger;

    public QueryService(
      IQueryCacheService queryCacheService,
      IEmbeddingService embeddingService,
      ISqlGeneratorService sqlGenerator,
      ISqlExecutorService sqlExecutor,
      IResponseFormatterService responseFormatter,
      ILogger<QueryService> logger
    )
    {
      _queryCacheService = queryCacheService;
      _embeddingService = embeddingService;
      _sqlGenerator = sqlGenerator;
      _sqlExecutor = sqlExecutor;
      _responseFormatter = responseFormatter;
      _logger = logger;
    }

    public async Task<ConsultaResponse> ConsultarAsync(string pergunta, CancellationToken cancellationToken = default)
    {
      var sqlCacheado = await _queryCacheService.BuscarSqlCacheadoAsync(pergunta, cancellationToken);

      string sql;
      List<Dictionary<string, object?>> dados;

      if (sqlCacheado != null)
      {
        _logger.LogInformation("Cache hit — usando SQL cacheado");
        sql = sqlCacheado;
        dados = await _sqlExecutor.ExecutarAsync(sql, cancellationToken);
      }
      else
      {
        var schemas = await _embeddingService.BuscarSchemasRelevantesAsync(pergunta, cancellationToken: cancellationToken);

        var contexto = string.Join("\n\n", schemas);

        (sql, dados) = await _sqlGenerator.GerarAsync(pergunta, contexto, cancellationToken);

        ValidarLeitura(sql);

        await _queryCacheService.SalvarAsync(pergunta, sql, cancellationToken);
      }

      var resposta = await _responseFormatter.FormatarAsync(pergunta, sql, dados, cancellationToken);

      return new ConsultaResponse
      {
        Pergunta = pergunta,
        SqlGerado = sql,
        Resposta = resposta,
        Dados = dados
      };
    }

    private static void ValidarLeitura(string sql)
    {
      var sqlUpper = sql.ToUpper().Trim();

      if (sqlUpper.StartsWith("INSERT") || sqlUpper.StartsWith("UPDATE") || sqlUpper.StartsWith("DELETE") || sqlUpper.StartsWith("DROP") || sqlUpper.StartsWith("TRUNCATE") || sqlUpper.StartsWith("ALTER"))
        throw new InvalidOperationException("Operações de escrita não são permitidas neste endpoint.");
    }
  }
}