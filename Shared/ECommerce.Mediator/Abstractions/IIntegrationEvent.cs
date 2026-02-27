namespace ECommerce.Mediator.Abstractions
{
  public interface IIntegrationEvent
  {
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    string EventType => GetType().Name;
  }
}
