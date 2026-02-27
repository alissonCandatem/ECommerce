using ECommerce.Mediator.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ECommerce.Produtos.Infrastructure.Outbox
{
  public sealed class OutboxProcessor : BackgroundService
  {
    private readonly IServiceProvider _provider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

    public OutboxProcessor(
      IServiceProvider provider,
      ILogger<OutboxProcessor> logger
    )
    {
      _provider = provider;
      _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Outbox Processor iniciado");

      while (!stoppingToken.IsCancellationRequested)
      {
        await ProcessAsync(stoppingToken);
        await Task.Delay(_interval, stoppingToken);
      }
    }

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
      using var scope = _provider.CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

      var messages = await context.OutboxMessages
      .Where(m => m.ProcessedAt == null && m.Error == null)
      .OrderBy(m => m.OccurredAt)
      .Take(20)
      .ToListAsync(cancellationToken);

      if (!messages.Any() || messages == null)
        return;

      _logger.LogInformation("Processando {Count} mensagens do outbox", messages.Count);

      foreach (var message in messages)
      {
        try
        {
          var eventType = Type.GetType(message.Type);

          if (eventType == null)
          {
            message.Error = $"Tipo não encontrado: {message.Type}";
            continue;
          }

          var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType) as IDomainEvent;

          if (domainEvent == null)
          {
            message.Error = "Falha ao deserializar o evento";
            continue;
          }

          await publisher.PublishAsync(domainEvent, cancellationToken);

          message.ProcessedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Erro ao processar mensagem {MessageId}", message.Id);
          message.Error = ex.Message;
        }
      }

      await context.SaveChangesAsync(cancellationToken);
    }
  }
}
