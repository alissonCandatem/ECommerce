using Confluent.Kafka;
using ECommerce.Mediator.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ECommerce.Pedidos.Infrastructure.Outbox
{
  public sealed class KafkaEventPublisher : IEventPublisher
  {
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(
      IConfiguration configuration,
      ILogger<KafkaEventPublisher> logger
    )
    {
      _logger = logger;

      var config = new ProducerConfig
      {
        BootstrapServers = configuration["Kafka:BootstrapServers"],
        Acks = Acks.All,
        EnableIdempotence = true
      };

      _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
      var eventType = domainEvent.GetType().Name;
      var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

      var message = new Message<string, string>
      {
        Key = domainEvent.EventId.ToString(),
        Value = payload,
        Headers = new Headers
        {
          { "event-type", System.Text.Encoding.UTF8.GetBytes(eventType) },
          { "occurred-at", System.Text.Encoding.UTF8.GetBytes(domainEvent.OccurredAt.ToString("O")) }
        }
      };

      var topic = eventType.ToLower().Replace("event", "");

      await _producer.ProduceAsync(topic, message, cancellationToken);

      _logger.LogInformation("Evento {EventType} publicado no tópico {Topic}", eventType, topic);
    }
  }
}
