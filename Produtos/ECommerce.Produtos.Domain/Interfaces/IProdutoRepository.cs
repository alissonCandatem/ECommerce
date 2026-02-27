using ECommerce.Produtos.Domain.Entities;

namespace ECommerce.Produtos.Domain.Interfaces
{
  public interface IProdutoRepository
  {
    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Produto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Produto>> ObterPorCategoriaAsync(string categoria, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Produto produto, CancellationToken cancellationToken = default);
    void Atualizar(Produto produto);
  }
}
