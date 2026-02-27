using ECommerce.Pedidos.Domain.Enums;
using ECommerce.Pedidos.Domain.Events;
using ECommerce.Pedidos.Domain.Primitives;

namespace ECommerce.Pedidos.Domain.Entities
{
  public sealed class Pedido : AggregateRoot
  {
    private readonly List<PedidoItem> _itens = [];

    public Guid UsuarioId { get; private init; }
    public EStatusPedido Status { get; private set; }
    public decimal Total => _itens.Sum(x => x.Total);
    public DateTime CriadoEm { get; private init; }
    public DateTime? AtualizadoEm { get; private set; }
    public IReadOnlyList<PedidoItem> Itens => _itens.AsReadOnly();

    private Pedido(Guid id) : base(id) { }

    public static Pedido Criar(Guid usuarioId, List<PedidoItem> itens)
    {
      var pedido = new Pedido(Guid.NewGuid())
      {
        UsuarioId = usuarioId,
        Status = EStatusPedido.Pendente,
        CriadoEm = DateTime.UtcNow
      };

      pedido._itens.AddRange(itens);

      pedido.RaiseDomainEvent(new PedidoCriadoEvent
      {
        PedidoId = pedido.Id,
        UsuarioId = usuarioId,
        Itens = itens.Select(i => new PedidoItemEvent
        {
          ProdutoId = i.ProdutoId,
          Quantidade = i.Quantidade
        }).ToList()
      });

      return pedido;
    }

    public void Confirmar()
    {
      Status = EStatusPedido.Confirmado;
      AtualizadoEm = DateTime.UtcNow;
    }

    public void Cancelar()
    {
      Status = EStatusPedido.Cancelado;
      AtualizadoEm = DateTime.UtcNow;

      RaiseDomainEvent(new PedidoCanceladoEvent
      {
        PedidoId = Id,
        UsuarioId = UsuarioId
      });
    }

    public void Entregar()
    {
      Status = EStatusPedido.Entregue;
      AtualizadoEm = DateTime.UtcNow;
    }
  }
}
