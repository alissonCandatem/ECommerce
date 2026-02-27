using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;

namespace ECommerce.Produtos.Application.Commands.Produto
{
  public sealed record AtualizarEstoqueCommand : ICommand<ResultNotification>
  {
    public Guid Id { get; init; }
    public int Quantidade { get; init; }
  }
}
