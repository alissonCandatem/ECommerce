using ECommerce.Mediator.Abstractions;

namespace ECommerce.Pedidos.Domain.Events
{
  public sealed record PedidoCanceladoEvent : IDomainEvent
  {
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid PedidoId { get; init; }
    public Guid UsuarioId { get; init; }
  }
}
