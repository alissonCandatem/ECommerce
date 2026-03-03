namespace ECommerce.IA.Api.Services.Interfaces
{
  public interface IEmbeddingService
  {
    Task<List<string>> BuscarSchemasRelevantesAsync(string pergunta, int limite = 5, CancellationToken cancellationToken = default);
  }
}
