using ECommerce.Mediator.Abstractions;

namespace ECommerce.Contracts.Contracts
{
  public sealed record PedidoCriadoContract : IDomainEvent
  {
    public Guid EventId { get; init; }
    public DateTime OccurredAt { get; init; }
    public Guid PedidoId { get; init; }
    public Guid UsuarioId { get; init; }
    public List<PedidoItemContract> Itens { get; init; } = [];
  }

  public sealed record PedidoItemContract
  {
    public Guid ProdutoId { get; init; }
    public int Quantidade { get; init; }
  }
}
