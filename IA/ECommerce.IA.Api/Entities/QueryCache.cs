using Pgvector;

namespace ECommerce.IA.Api.Entities
{
  public sealed class QueryCache
  {
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Pergunta { get; init; } = null!;
    public string SqlGerado { get; init; } = null!;
    public Vector Embedding { get; init; } = null!;
    public bool Sucesso { get; init; } = true;
    public DateTime CriadoEm { get; init; } = DateTime.UtcNow;
  }
}
