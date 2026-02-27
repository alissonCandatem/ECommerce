using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;
using ECommerce.Pedidos.Application.Queries;
using ECommerce.Pedidos.Application.Responses;
using ECommerce.Pedidos.Domain.Interfaces;

namespace ECommerce.Pedidos.Application.Handlers.Pedidos.QueryHandlers
{
  public sealed class PedidoQueryHandlers : ICommandHandler<ObterPedidosQuery, Result<List<PedidoResponse>>>
  {
    private readonly IPedidoRepository _repository;

    public PedidoQueryHandlers(IPedidoRepository repository)
    {
      _repository = repository;
    }

    public async Task<Result<List<PedidoResponse>>> Handle(ObterPedidosQuery query, CancellationToken cancellationToken)
    {
      var pedidos = await _repository.ObterPorUsuarioAsync(query.UsuarioId, cancellationToken);

      var response = pedidos.Select(p => new PedidoResponse
      {
        Id = p.Id,
        UsuarioId = p.UsuarioId,
        Status = p.Status,
        Total = p.Total,
        CriadoEm = p.CriadoEm,
        Itens = p.Itens.Select(i => new PedidoItemResponse
        {
          ProdutoId = i.ProdutoId,
          NomeProduto = i.NomeProduto,
          PrecoUnitario = i.PrecoUnitario,
          Quantidade = i.Quantidade,
          Total = i.Total
        }).ToList()
      }).ToList();

      return Result<List<PedidoResponse>>.Ok(response);
    }
  }
}
