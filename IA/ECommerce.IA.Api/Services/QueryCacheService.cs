using ECommerce.IA.Api.Data;
using ECommerce.IA.Api.Entities;
using ECommerce.IA.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.IA.Api.Services
{
  public sealed class QueryCacheService : IQueryCacheService
  {
    private readonly IADbContext _context;
    private readonly IOllamaService _ollamaService;
    private readonly ILogger<QueryCacheService> _logger;
    private const float SimilaridadeMinima = 0.85f; // A similaridade vai impactar no SQL gerado, se uma frase for 85% "similar" a uma que ja foi cacheada, vai gerar o SQL(Obs: Isso não é a melhor forma pois podem gerar confusão dependendo do Modelo)

    public QueryCacheService(
      IADbContext context,
      IOllamaService ollamaService,
      ILogger<QueryCacheService> logger
    )
    {
      _context = context;
      _ollamaService = ollamaService;
      _logger = logger;
    }

    public async Task<string?> BuscarSqlCacheadoAsync(string pergunta, CancellationToken cancellationToken = default)
    {
      var embedding = await _ollamaService.GerarEmbeddingAsync(pergunta, cancellationToken);

      var cache = await _context.QueryCache
      .FromSqlRaw("""
        SELECT id, pergunta, sql_gerado, embedding, sucesso, criado_em
        FROM query_cache
        WHERE sucesso = true
        ORDER BY embedding <-> {0}
        LIMIT 5
       """, embedding)
      .FirstOrDefaultAsync(cancellationToken);

      if (cache == null)
        return null;

      var similaridade = CalcularSimilaridade(embedding.ToArray(), cache.Embedding.ToArray());

      if (similaridade >= SimilaridadeMinima)
      {
        _logger.LogInformation($"Cache hit! Similaridade: {similaridade:P2} | Pergunta cacheada: {cache.Pergunta}");
        return cache.SqlGerado;
      }

      _logger.LogInformation($"Cache miss. Similaridade mais próxima: {similaridade:P2}");
      return null;
    }

    public async Task SalvarAsync(string pergunta, string sql, CancellationToken cancellationToken = default)
    {
      var embedding = await _ollamaService.GerarEmbeddingAsync(pergunta, cancellationToken);

      _context.QueryCache.Add(new QueryCache
      {
        Pergunta = pergunta,
        SqlGerado = sql,
        Embedding = embedding,
        Sucesso = true
      });

      await _context.SaveChangesAsync(cancellationToken);
      _logger.LogInformation($"Query salva no cache: {pergunta}");
    }

    private static float CalcularSimilaridade(float[] a, float[] b)
    {
      var dotProduct = a.Zip(b, (x, y) => x * y).Sum();
      var magnitudeA = MathF.Sqrt(a.Sum(x => x * x));
      var magnitudeB = MathF.Sqrt(b.Sum(x => x * x));
      return dotProduct / (magnitudeA * magnitudeB);
    }
  }
}
