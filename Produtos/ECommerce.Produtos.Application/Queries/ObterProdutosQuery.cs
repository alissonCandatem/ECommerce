using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;
using ECommerce.Produtos.Application.Responses;

namespace ECommerce.Produtos.Application.Queries
{
  public sealed class ObterProdutosQuery : ICommand<Result<List<ProdutoResponse>>>
  {
    public string? Categoria { get; init; }
  }
}
