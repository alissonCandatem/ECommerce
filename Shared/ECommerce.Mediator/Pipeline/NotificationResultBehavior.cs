using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;

namespace ECommerce.Mediator.Pipeline
{
  public sealed class NotificationResultBehavior<TCommand> : INotificationBehavior<TCommand> where TCommand : ICommand<ResultNotification>
  {
    private readonly INotificationContext _notification;

    public NotificationResultBehavior(INotificationContext notification)
    {
      _notification = notification;
    }

    public async Task<ResultNotification> Handle(TCommand command, CancellationToken cancellationToken, Func<Task<ResultNotification>> next)
    {
      var result = await next();

      if (!_notification.Notifications.Any())
        return result;

      return ResultNotification.FromContext(_notification);
    }
  }
}
