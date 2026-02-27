using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared.Enums;

namespace ECommerce.Mediator.Shared
{
  public sealed class NotificationContext : INotificationContext
  {
    private readonly List<Notification> _notifications = new();
    public IReadOnlyCollection<Notification> Notifications => _notifications;

    public bool HasErrors => _notifications.Any(x => x.Type == ENotificationType.Error);
    public bool HasWarnings => _notifications.Any(x => x.Type == ENotificationType.Warning);

    public void AddError(string code, string message)
        => _notifications.Add(new Notification(code, message, ENotificationType.Error));

    public void AddError(string message)
      => _notifications.Add(new Notification(message, ENotificationType.Error));

    public void AddWarning(string code, string message)
        => _notifications.Add(new Notification(code, message, ENotificationType.Warning));

    public void AddWarning(string message)
      => _notifications.Add(new Notification(message, ENotificationType.Warning));
  }
}
