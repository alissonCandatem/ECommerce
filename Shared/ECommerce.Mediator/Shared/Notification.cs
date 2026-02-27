using ECommerce.Mediator.Shared.Enums;

namespace ECommerce.Mediator.Shared
{
  public sealed class Notification
  {
    public string? Code { get; }
    public string Message { get; }
    public ENotificationType? Type { get; }

    public Notification(string code, string message, ENotificationType type)
    {
      Code = code;
      Message = message;
      Type = type;
    }

    public Notification(string message, ENotificationType type)
    {
      Message = message;
      Type = type;
    }
  }
}
