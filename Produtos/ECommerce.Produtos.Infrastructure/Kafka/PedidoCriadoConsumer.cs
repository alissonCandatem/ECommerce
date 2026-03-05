using Confluent.Kafka;
using ECommerce.Contracts.Contracts;
using ECommerce.Mediator.Abstractions;
using ECommerce.Produtos.Application.Commands.Produto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ECommerce.Produtos.Infrastructure.Kafka
{
  public sealed class PedidoCriadoConsumer : BackgroundService
  {
    private readonly IServiceProvider _provider;
    private readonly ILogger<PedidoCriadoConsumer> _logger;
    private readonly IConsumer<string, string> _consumer;
    private const string Topic = "pedidocriado";

    public PedidoCriadoConsumer(
      IServiceProvider provider,
      IConfiguration configuration,
      ILogger<PedidoCriadoConsumer> logger
    )
    {
      _provider = provider;
      _logger = logger;

      var config = new ConsumerConfig
      {
        BootstrapServers = configuration["Kafka:BootstrapServers"],
        GroupId = "produtos-consumer-group",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false // commit manual para garantir que processou
      };

      _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _consumer.Subscribe(Topic);
      _logger.LogInformation($"Consumer inscrito no tópico {Topic}");

      while (!stoppingToken.IsCancellationRequested)
      {
        try
        {
          var result = _consumer.Consume(stoppingToken);
          if (result == null) continue;

          _logger.LogInformation($"Mensagem recebida do tópico {Topic}");

          await ProcessAsync(result.Message.Value, stoppingToken);

          _consumer.Commit(result);
        }
        catch (OperationCanceledException)
        {
          break;
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, $"Erro ao consumir mensagem do tópico {Topic}");
        }
      }

      _consumer.Close();
    }

    private async Task ProcessAsync(string payload, CancellationToken cancellationToken)
    {
      var contract = JsonSerializer.Deserialize<PedidoCriadoContract>(payload);
      if (contract == null)
      {
        _logger.LogWarning("Falha ao deserializar PedidoCriadoContract");
        return;
      }

      using var scope = _provider.CreateScope();
      var dispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

      foreach (var item in contract.Itens)
      {
        var command = new AtualizarEstoqueCommand
        {
          Id = item.ProdutoId,
          Quantidade = -item.Quantidade
        };

        var result = await dispatcher.Send(command, cancellationToken);

        if (!result.IsSuccess)
          _logger.LogWarning($"Falha ao atualizar estoque do produto {item.ProdutoId}: " + string.Join(", ", result.Notifications.Select(n => n.Message)));
        else
          _logger.LogInformation($"Estoque do produto {item.ProdutoId} atualizado. Quantidade: -{item.Quantidade}");
      }
    }
  }
}
