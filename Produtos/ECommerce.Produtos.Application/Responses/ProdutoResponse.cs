namespace ECommerce.Produtos.Application.Responses
{
  public sealed class ProdutoResponse
  {
    public Guid Id { get; init; }
    public string Nome { get; init; } = null!;
    public string Descricao { get; init; } = null!;
    public decimal Preco { get; init; }
    public int Estoque { get; init; }
    public string Categoria { get; init; } = null!;
    public bool Ativo { get; init; }
    public DateTime CriadoEm { get; init; }
  }
}
