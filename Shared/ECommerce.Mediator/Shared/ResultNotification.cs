using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared.Enums;

namespace ECommerce.Mediator.Shared
{
  public sealed class ResultNotification
  {
    public bool IsSuccess { get; }

    public IReadOnlyCollection<Notification> Notifications { get; }

    private ResultNotification(IReadOnlyCollection<Notification> notifications)
    {
      Notifications = notifications;
      IsSuccess = !notifications.Any(x => x.Type == ENotificationType.Error);
    }

    public static ResultNotification Ok() => new([]);

    public static ResultNotification Fail(INotificationContext context) => FromContext(context);

    public static ResultNotification FromContext(INotificationContext context)
       => new(context.Notifications);
  }
}
