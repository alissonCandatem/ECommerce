using ECommerce.Mediator.Abstractions;
using ECommerce.Pedidos.Application.Commands;
using ECommerce.Pedidos.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Pedidos.Api.Controllers
{
  [Route("api/pedidos")]
  [ApiController]
  [Authorize]
  public class PedidosController : ControllerBase
  {
    private readonly ICommandDispatcher _dispatcher;

    public PedidosController(ICommandDispatcher dispatcher)
    {
      _dispatcher = dispatcher;
    }

    [HttpGet]
    public async Task<IActionResult> ObterMeusPedidos()
    {
      var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
      return Ok(await _dispatcher.Send(new ObterPedidosQuery { UsuarioId = usuarioId }));
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarPedidoCommand command)
    {
      var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
      var cmd = command with { UsuarioId = usuarioId };
      return Ok(await _dispatcher.Send(cmd));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancelar(Guid id)
    {
      var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
      return Ok(await _dispatcher.Send(new CancelarPedidoCommand
      {
        Id = id,
        UsuarioId = usuarioId
      }));
    }
  }
}
