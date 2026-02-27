
using ECommerce.Mediator.Shared;

namespace ECommerce.Mediator.Abstractions
{
  public interface INotificationContext
  {
    IReadOnlyCollection<Notification> Notifications { get; }
    bool HasErrors { get; }
    bool HasWarnings { get; }

    void AddError(string message);
    void AddError(string code, string message);
    void AddWarning(string message);
    void AddWarning(string code, string message);

    void AddNotFound(string message) => AddError("NotFound", message);
    void AddConflict(string message) => AddError("Conflict", message);
    void AddUnauthorized(string message) => AddError("Unauthorized", message);
    void AddForbidden(string message) => AddError("Forbidden", message);
  }
}
