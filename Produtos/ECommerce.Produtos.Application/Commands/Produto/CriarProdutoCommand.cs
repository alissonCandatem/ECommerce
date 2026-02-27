using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;

namespace ECommerce.Produtos.Application.Commands.Produto
{
  public sealed record CriarProdutoCommand : ICommand<ResultNotification>
  {
    public string Nome { get; init; } = null!;
    public string Descricao { get; init; } = null!;
    public decimal Preco { get; init; }
    public int Estoque { get; init; }
    public string Categoria { get; init; } = null!;
  }
}
