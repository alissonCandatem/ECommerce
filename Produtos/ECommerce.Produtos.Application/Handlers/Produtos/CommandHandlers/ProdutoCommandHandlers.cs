using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;
using ECommerce.Produtos.Application.Commands.Produto;
using ECommerce.Produtos.Domain.Entities;
using ECommerce.Produtos.Domain.Interfaces;

namespace ECommerce.Produtos.Application.Handlers.Produtos.CommandHandlers
{
  public sealed class ProdutoCommandHandlers :
    ICommandHandler<CriarProdutoCommand, ResultNotification>,
    ICommandHandler<AtualizarProdutoCommand, ResultNotification>,
    ICommandHandler<AtualizarEstoqueCommand, ResultNotification>
  {

    private readonly IProdutoRepository _repository;
    private readonly INotificationContext _notification;
    public ProdutoCommandHandlers(
      IProdutoRepository repository,
      INotificationContext notification
    )
    {
      _repository = repository;
      _notification = notification;
    }

    public async Task<ResultNotification> Handle(CriarProdutoCommand command, CancellationToken cancellationToken)
    {
      if (command.Preco <= 0)
      {
        _notification.AddError("Preço deve ser maior que zero");
        return ResultNotification.Fail(_notification);
      }

      if (command.Estoque < 0)
      {
        _notification.AddError("Estoque não pode ser negativo");
        return ResultNotification.Fail(_notification);
      }

      var produto = Produto.Criar(
        command.Nome,
        command.Descricao,
        command.Preco,
        command.Estoque,
        command.Categoria
      );

      await _repository.AdicionarAsync(produto, cancellationToken);

      return ResultNotification.Ok();
    }

    public async Task<ResultNotification> Handle(AtualizarProdutoCommand command, CancellationToken cancellationToken)
    {
      var produto = await _repository.ObterPorIdAsync(command.Id, cancellationToken);

      if (produto == null)
      {
        _notification.AddNotFound("Produto não encontrado");
        return ResultNotification.Fail(_notification);
      }

      produto.Atualizar(command.Nome, command.Descricao, command.Preco, command.Categoria);
      _repository.Atualizar(produto);

      return ResultNotification.Ok();
    }

    public async Task<ResultNotification> Handle(AtualizarEstoqueCommand command, CancellationToken cancellationToken)
    {
      var produto = await _repository.ObterPorIdAsync(command.Id, cancellationToken);

      if (produto == null)
      {
        _notification.AddNotFound("Produto não encontrado");
        return ResultNotification.Fail(_notification);
      }

      if (produto.Estoque + command.Quantidade < 0)
      {
        _notification.AddError("Estoque insuficiente");
        return ResultNotification.Fail(_notification);
      }

      produto.AtualizarEstoque(command.Quantidade);
      _repository.Atualizar(produto);

      return ResultNotification.Ok();
    }
  }
}
