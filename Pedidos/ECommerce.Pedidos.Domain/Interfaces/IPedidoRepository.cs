using ECommerce.Pedidos.Domain.Entities;

namespace ECommerce.Pedidos.Domain.Interfaces
{
  public interface IPedidoRepository
  {
    Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Pedido>> ObterPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken = default);
    void Atualizar(Pedido pedido);
  }
}
