using ECommerce.Mediator.Shared;

namespace ECommerce.Mediator.Abstractions
{
  public interface ICommandNotificationValidator<TCommand>
  {
    IEnumerable<Notification> Validate(TCommand command);
  }
}
