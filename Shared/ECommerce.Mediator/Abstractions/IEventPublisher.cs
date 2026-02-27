namespace ECommerce.Mediator.Abstractions
{
  public interface IEventPublisher
  {
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
  }
}
