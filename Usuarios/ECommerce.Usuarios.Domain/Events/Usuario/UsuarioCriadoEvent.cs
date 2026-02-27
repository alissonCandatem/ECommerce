using ECommerce.Mediator.Abstractions;

namespace ECommerce.Usuarios.Domain.Events.Usuario
{
  public sealed record UsuarioCriadoEvent : IDomainEvent
  {
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid UserId { get; init; }
    public string Email { get; init; } = null!;
  }
}
