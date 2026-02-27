using ECommerce.Mediator.Abstractions;

namespace ECommerce.Produtos.Domain.Events
{
  public sealed record ProdutoCriadoEvent : IDomainEvent
  {
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid ProdutoId { get; init; }
    public string Nome { get; init; } = null!;
  }
}
