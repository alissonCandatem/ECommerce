using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;

namespace ECommerce.Pedidos.Application.Commands
{
  public sealed record CriarPedidoCommand : ICommand<ResultNotification>
  {
    public Guid UsuarioId { get; init; }
    public List<CriarPedidoItemCommand> Itens { get; init; } = [];
  }

  public sealed record CriarPedidoItemCommand
  {
    public Guid ProdutoId { get; init; }
    public string NomeProduto { get; init; } = null!;
    public decimal PrecoUnitario { get; init; }
    public int Quantidade { get; init; }
  }
}
