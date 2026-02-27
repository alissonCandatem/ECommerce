using ECommerce.Pedidos.Domain.Primitives;

namespace ECommerce.Pedidos.Domain.Entities
{
  public sealed class PedidoItem : Entity
  {
    public Guid PedidoId { get; private init; }
    public Guid ProdutoId { get; private init; }
    public string NomeProduto { get; private init; } = null!;
    public decimal PrecoUnitario { get; private init; }
    public int Quantidade { get; private init; }
    public decimal Total => PrecoUnitario * Quantidade;

    private PedidoItem(Guid id) : base(id) { }

    public static PedidoItem Criar(
        Guid pedidoId,
        Guid produtoId,
        string nomeProduto,
        decimal precoUnitario,
        int quantidade)
    {
      return new PedidoItem(Guid.NewGuid())
      {
        PedidoId = pedidoId,
        ProdutoId = produtoId,
        NomeProduto = nomeProduto,
        PrecoUnitario = precoUnitario,
        Quantidade = quantidade
      };
    }
  }
}
