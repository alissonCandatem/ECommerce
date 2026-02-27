using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;
using ECommerce.Usuarios.Application.Responses;

namespace ECommerce.Usuarios.Application.Commands
{
  public sealed record LoginCommand : ICommand<Result<LoginResponse>>
  {
    public string Email { get; init; } = null!;
    public string Senha { get; init; } = null!;
  }
}
