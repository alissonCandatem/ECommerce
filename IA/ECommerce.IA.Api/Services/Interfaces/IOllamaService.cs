using Pgvector;

namespace ECommerce.IA.Api.Services.Interfaces
{
  public interface IOllamaService
  {
    Task<Vector> GerarEmbeddingAsync(string texto, CancellationToken cancellationToken = default);
    Task<string> GerarSqlAsync(string prompt, CancellationToken cancellationToken = default);
    Task<string> GerarRespostaAsync(string prompt, CancellationToken cancellationToken = default);
  }
}
