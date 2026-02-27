using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;
using ECommerce.Produtos.Application.Queries;
using ECommerce.Produtos.Application.Responses;
using ECommerce.Produtos.Domain.Interfaces;

namespace ECommerce.Produtos.Application.Handlers.Produtos.QueryHandlers
{
  public sealed class ProdutoQueryHandlers : ICommandHandler<ObterProdutosQuery, Result<List<ProdutoResponse>>>
  {
    private readonly IProdutoRepository _repository;

    public ProdutoQueryHandlers(IProdutoRepository repository)
    {
      _repository = repository;
    }

    public async Task<Result<List<ProdutoResponse>>> Handle(
        ObterProdutosQuery query,
        CancellationToken cancellationToken)
    {
      var produtos = string.IsNullOrWhiteSpace(query.Categoria)
          ? await _repository.ObterTodosAsync(cancellationToken)
          : await _repository.ObterPorCategoriaAsync(query.Categoria, cancellationToken);

      var response = produtos.Select(p => new ProdutoResponse
      {
        Id = p.Id,
        Nome = p.Nome,
        Descricao = p.Descricao,
        Preco = p.Preco,
        Estoque = p.Estoque,
        Categoria = p.Categoria,
        Ativo = p.Ativo,
        CriadoEm = p.CriadoEm
      }).ToList();

      return Result<List<ProdutoResponse>>.Ok(response);
    }
  }
}
