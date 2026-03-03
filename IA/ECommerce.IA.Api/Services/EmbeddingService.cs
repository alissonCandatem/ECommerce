using ECommerce.IA.Api.Data;
using ECommerce.IA.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.IA.Api.Services
{
  public sealed class EmbeddingService : IEmbeddingService
  {
    private readonly IADbContext _context;
    private readonly IOllamaService _ollamaService;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(
      IADbContext context,
      IOllamaService ollamaService,
      ILogger<EmbeddingService> logger
    )
    {
      _context = context;
      _ollamaService = ollamaService;
      _logger = logger;
    }

    public async Task<List<string>> BuscarSchemasRelevantesAsync(string pergunta, int limite = 5, CancellationToken cancellationToken = default)
    {
      _logger.LogInformation($"Buscando schemas relevantes para: {pergunta}");

      var embedding = await _ollamaService.GerarEmbeddingAsync(pergunta, cancellationToken);

      var schemas = await _context.SchemaEmbeddings
      .FromSqlRaw("""
        SELECT id, tabela, banco, descricao, embedding, criado_em
        FROM schema_embeddings
        ORDER BY embedding <-> {0}
        LIMIT {1}
       """,
       embedding,
       limite
      ).ToListAsync(cancellationToken);

      _logger.LogInformation($"Schemas encontrados: {string.Join(", ", schemas.Select(s => $"{s.Banco}.{s.Tabela}"))}");

      return schemas.Select(s => s.Descricao).ToList();
    }
  }
}
