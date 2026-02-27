using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;
using ECommerce.Pedidos.Application.Responses;

namespace ECommerce.Pedidos.Application.Queries
{
  public sealed record ObterPedidosQuery : ICommand<Result<List<PedidoResponse>>>
  {
    public Guid UsuarioId { get; init; }
  }
}
