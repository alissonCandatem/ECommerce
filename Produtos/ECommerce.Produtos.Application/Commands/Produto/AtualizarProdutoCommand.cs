using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;

namespace ECommerce.Produtos.Application.Commands.Produto
{
  public sealed record AtualizarProdutoCommand : ICommand<ResultNotification>
  {
    public Guid Id { get; init; }
    public string Nome { get; init; } = null!;
    public string Descricao { get; init; } = null!;
    public decimal Preco { get; init; }
    public string Categoria { get; init; } = null!;
  }
}
