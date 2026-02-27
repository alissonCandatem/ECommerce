using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;

namespace ECommerce.Pedidos.Application.Commands
{
  public sealed record CancelarPedidoCommand : ICommand<ResultNotification>
  {
    public Guid Id { get; init; }
    public Guid UsuarioId { get; init; }
  }
}
