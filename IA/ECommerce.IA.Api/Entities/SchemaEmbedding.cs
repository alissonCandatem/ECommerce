using Pgvector;

namespace ECommerce.IA.Api.Entities
{
  public sealed class SchemaEmbedding
  {
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Tabela { get; init; } = null!;
    public string Banco { get; init; } = null!;
    public string Descricao { get; init; } = null!;
    public Vector Embedding { get; init; } = null!;
    public DateTime CriadoEm { get; init; } = DateTime.UtcNow;
  }
}
