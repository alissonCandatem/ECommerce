using ECommerce.Pedidos.Domain.Enums;

namespace ECommerce.Pedidos.Application.Responses
{
  public sealed class PedidoResponse
  {
    public Guid Id { get; init; }
    public Guid UsuarioId { get; init; }
    public EStatusPedido Status { get; init; }
    public decimal Total { get; init; }
    public DateTime CriadoEm { get; init; }
    public List<PedidoItemResponse> Itens { get; init; } = [];
  }

  public sealed class PedidoItemResponse
  {
    public Guid ProdutoId { get; init; }
    public string NomeProduto { get; init; } = null!;
    public decimal PrecoUnitario { get; init; }
    public int Quantidade { get; init; }
    public decimal Total { get; init; }
  }
}
