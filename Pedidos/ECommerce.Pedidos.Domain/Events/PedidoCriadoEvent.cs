using ECommerce.Mediator.Abstractions;

namespace ECommerce.Pedidos.Domain.Events
{
  public sealed record PedidoCriadoEvent : IDomainEvent
  {
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid PedidoId { get; init; }
    public Guid UsuarioId { get; init; }
    public List<PedidoItemEvent> Itens { get; init; } = [];
  }

  public sealed record PedidoItemEvent
  {
    public Guid ProdutoId { get; init; }
    public int Quantidade { get; init; }
  }
}
