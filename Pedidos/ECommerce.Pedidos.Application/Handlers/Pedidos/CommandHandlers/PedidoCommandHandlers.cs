using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;
using ECommerce.Pedidos.Application.Commands;
using ECommerce.Pedidos.Domain.Entities;
using ECommerce.Pedidos.Domain.Enums;
using ECommerce.Pedidos.Domain.Interfaces;

namespace ECommerce.Pedidos.Application.Handlers.Pedidos.CommandHandlers
{
  public sealed class PedidoCommandHandlers :
    ICommandHandler<CriarPedidoCommand, ResultNotification>,
    ICommandHandler<CancelarPedidoCommand, ResultNotification>
  {
    private readonly IPedidoRepository _repository;
    private readonly INotificationContext _notification;

    public PedidoCommandHandlers(
      IPedidoRepository repository,
      INotificationContext notification
    )
    {
      _repository = repository;
      _notification = notification;
    }

    public async Task<ResultNotification> Handle(CriarPedidoCommand command, CancellationToken cancellationToken)
    {
      if (!command.Itens.Any())
      {
        _notification.AddError("Pedido deve ter pelo menos um item");
        return ResultNotification.Fail(_notification);
      }

      var itens = command.Itens.Select(i => PedidoItem.Criar(
        Guid.Empty,
        i.ProdutoId,
        i.NomeProduto,
        i.PrecoUnitario,
        i.Quantidade))
      .ToList();

      var pedido = Pedido.Criar(command.UsuarioId, itens);

      await _repository.AdicionarAsync(pedido, cancellationToken);

      return ResultNotification.Ok();
    }

    public async Task<ResultNotification> Handle(CancelarPedidoCommand command, CancellationToken cancellationToken)
    {
      var pedido = await _repository.ObterPorIdAsync(command.Id, cancellationToken);
      if (pedido == null)
      {
        _notification.AddNotFound("Pedido não encontrado");
        return ResultNotification.Fail(_notification);
      }

      if (pedido.UsuarioId != command.UsuarioId)
      {
        _notification.AddForbidden("Você não tem permissão para cancelar este pedido");
        return ResultNotification.Fail(_notification);
      }

      if (pedido.Status == EStatusPedido.Entregue)
      {
        _notification.AddError("Pedido já entregue não pode ser cancelado");
        return ResultNotification.Fail(_notification);
      }

      pedido.Cancelar();
      _repository.Atualizar(pedido);

      return ResultNotification.Ok();
    }
  }
}
