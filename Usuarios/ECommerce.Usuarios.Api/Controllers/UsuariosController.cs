using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;
using ECommerce.Usuarios.Application.Commands;
using ECommerce.Usuarios.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Usuarios.Api.Controllers
{
  [Route("api/usuario")]
  [ApiController]
  public class UsuariosController : ControllerBase
  {
    private readonly ICommandDispatcher _dispatcher;

    public UsuariosController(ICommandDispatcher dispatcher)
    {
      _dispatcher = dispatcher;
    }

    [HttpPost("registrar")]
    public async Task<ResultNotification> Registrar(CriarUsuarioCommand command)
      => await _dispatcher.Send(command);

    [HttpPost("login")]
    public async Task<Result<LoginResponse>> Login(LoginCommand command)
      => await _dispatcher.Send(command);

    [HttpPost("refresh-token")]
    public async Task<Result<LoginResponse>> RefreshToken(RefreshTokenCommand command)
      => await _dispatcher.Send(command);
  }
}
