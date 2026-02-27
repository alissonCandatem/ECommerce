using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;

namespace ECommerce.Usuarios.Application.Commands
{
  public record CriarUsuarioCommand : ICommand<ResultNotification>
  {
    public string Nome { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string Senha { get; init; } = null!;
  }
}
