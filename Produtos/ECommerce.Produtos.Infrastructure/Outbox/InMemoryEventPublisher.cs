using ECommerce.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Produtos.Infrastructure.Outbox
{
  public sealed class InMemoryEventPublisher : IEventPublisher
  {
    private readonly IServiceProvider _provider;
    private readonly ILogger<InMemoryEventPublisher> _logger;

    public InMemoryEventPublisher(
      IServiceProvider provider,
      ILogger<InMemoryEventPublisher> logger
    )
    {
      _provider = provider;
      _logger = logger;
    }

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
      var eventType = domainEvent.GetType();
      var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
      var handlers = _provider.GetServices(handlerType).ToList();

      if (!handlers.Any())
      {
        _logger.LogInformation("Nenhum handler encontrado para o evento {EventType}", eventType.Name);
        return;
      }

      foreach (dynamic? handler in handlers)
      {
        if (handler == null) continue;

        await handler.Handle((dynamic)domainEvent, cancellationToken);

        _logger.LogInformation($"Evento {eventType.Name} processado pelo handler {handler.GetType().Name}");
      }
    }
  }
}
