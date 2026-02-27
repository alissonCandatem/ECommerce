using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;
using ECommerce.Usuarios.Application.Responses;

namespace ECommerce.Usuarios.Application.Commands
{
  public sealed record RefreshTokenCommand : ICommand<Result<LoginResponse>>
  {
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
  }
}
