using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;

namespace ECommerce.Usuarios.Application.Commands
{
  public record AlterarUsuarioCommand : ICommand<ResultNotification>
  {
    public string? Nome { get; set; }
    public int Idade { get; set; }
  }
}
