using ECommerce.Produtos.Domain.Events;
using ECommerce.Produtos.Domain.Primitives;

namespace ECommerce.Produtos.Domain.Entities
{
  public sealed class Produto : AggregateRoot
  {
    public string Nome { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public decimal Preco { get; private set; }
    public int Estoque { get; private set; }
    public string Categoria { get; private set; } = null!;
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private init; }

    private Produto(Guid id) : base(id) { }

    public static Produto Criar(
      string nome,
      string descricao,
      decimal preco,
      int estoque,
      string categoria
    )
    {
      var produto = new Produto(Guid.NewGuid())
      {
        Nome = nome,
        Descricao = descricao,
        Preco = preco,
        Estoque = estoque,
        Categoria = categoria,
        Ativo = true,
        CriadoEm = DateTime.UtcNow
      };

      produto.RaiseDomainEvent(new ProdutoCriadoEvent
      {
        ProdutoId = produto.Id,
        Nome = nome
      });

      return produto;
    }

    public void AtualizarEstoque(int quantidade)
    {
      var estoqueAnterior = Estoque;
      Estoque += quantidade;

      RaiseDomainEvent(new EstoqueAtualizadoEvent
      {
        ProdutoId = Id,
        EstoqueAnterior = estoqueAnterior,
        EstoqueAtual = Estoque
      });
    }

    public void Atualizar(string nome, string descricao, decimal preco, string categoria)
    {
      Nome = nome;
      Descricao = descricao;
      Preco = preco;
      Categoria = categoria;
    }

    public void Desativar() => Ativo = false;
    public void Ativar() => Ativo = true;
  }
}
