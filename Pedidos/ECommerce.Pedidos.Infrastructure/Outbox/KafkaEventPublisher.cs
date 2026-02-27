using Confluent.Kafka;
using ECommerce.Contracts.Contracts;
using ECommerce.Mediator.Abstractions;
using ECommerce.Pedidos.Domain.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace ECommerce.Pedidos.Infrastructure.Outbox
{
  public sealed class KafkaEventPublisher : IEventPublisher
  {
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(IConfiguration configuration, ILogger<KafkaEventPublisher> logger)
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
      // mapeia o evento de domínio para o contrato compartilhado
      var (topic, payload) = domainEvent switch
      {
        PedidoCriadoEvent e => ("pedidocriado",
        JsonSerializer.Serialize(new PedidoCriadoContract
        {
          EventId = e.EventId,
          OccurredAt = e.OccurredAt,
          PedidoId = e.PedidoId,
          UsuarioId = e.UsuarioId,
          Itens = e.Itens.Select(i => new PedidoItemContract
          {
            ProdutoId = i.ProdutoId,
            Quantidade = i.Quantidade
          }).ToList()
        })
        ),
        _ => (domainEvent.GetType().Name.ToLower().Replace("event", ""), JsonSerializer.Serialize(domainEvent, domainEvent.GetType()))
      };

      var message = new Message<string, string>
      {
        Key = domainEvent.EventId.ToString(),
        Value = payload,
        Headers = new Headers
        {
          { "event-type", Encoding.UTF8.GetBytes(domainEvent.GetType().Name) },
          { "occurred-at", Encoding.UTF8.GetBytes(domainEvent.OccurredAt.ToString("O")) }
        }
      };

      await _producer.ProduceAsync(topic, message, cancellationToken);

      _logger.LogInformation($"Evento {domainEvent.GetType().Name} publicado no tópico {topic}");
    }
  }
}
