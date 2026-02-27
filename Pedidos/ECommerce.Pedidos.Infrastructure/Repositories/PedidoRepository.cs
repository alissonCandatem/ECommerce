using ECommerce.Pedidos.Domain.Entities;
using ECommerce.Pedidos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Pedidos.Infrastructure.Repositories
{
  public sealed class PedidoRepository : IPedidoRepository
  {
    private readonly AppDbContext _context;

    public PedidoRepository(AppDbContext context)
    {
      _context = context;
    }

    public async Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Pedidos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Pedido>> ObterPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
        => await _context.Pedidos.Where(x => x.UsuarioId == usuarioId).ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken = default)
        => await _context.Pedidos.AddAsync(pedido, cancellationToken);

    public void Atualizar(Pedido pedido)
        => _context.Pedidos.Update(pedido);
  }
}
