using ECommerce.Produtos.Domain.Entities;
using ECommerce.Produtos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Produtos.Infrastructure.Repositories
{
  public sealed class ProdutoRepository : IProdutoRepository
  {
    private readonly AppDbContext _context;

    public ProdutoRepository(AppDbContext context)
    {
      _context = context;
    }

    public async Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Produtos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Produto>> ObterTodosAsync(CancellationToken cancellationToken = default)
        => await _context.Produtos.Where(x => x.Ativo).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Produto>> ObterPorCategoriaAsync(string categoria, CancellationToken cancellationToken = default)
        => await _context.Produtos.Where(x => x.Categoria == categoria && x.Ativo).ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Produto produto, CancellationToken cancellationToken = default)
        => await _context.Produtos.AddAsync(produto, cancellationToken);

    public void Atualizar(Produto produto)
        => _context.Produtos.Update(produto);
  }
}
