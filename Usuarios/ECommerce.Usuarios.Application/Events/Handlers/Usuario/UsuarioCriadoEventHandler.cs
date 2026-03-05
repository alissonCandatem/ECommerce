using ECommerce.Mediator.Abstractions;
using ECommerce.Usuarios.Domain.Events.Usuario;
using Microsoft.Extensions.Logging;

namespace ECommerce.Usuarios.Application.Events.Handlers.Usuario
{
  public sealed class UsuarioCriadoEventHandler : IDomainEventHandler<UsuarioCriadoEvent>
  {
    private readonly ILogger<UsuarioCriadoEventHandler> _logger;

    public UsuarioCriadoEventHandler(ILogger<UsuarioCriadoEventHandler> logger)
    {
      _logger = logger;
    }

    public Task Handle(UsuarioCriadoEvent domainEvent, CancellationToken cancellationToken)
    {
      _logger.LogInformation($"Usuário criado: {domainEvent.UserId} - {domainEvent.Email}");

      return Task.CompletedTask;
    }
  }
}
