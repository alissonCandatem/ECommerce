namespace ECommerce.Mediator.Abstractions
{
  public interface IKafkaConsumer
  {
    Task ConsumeAsync(CancellationToken cancellationToken);
  }
}
