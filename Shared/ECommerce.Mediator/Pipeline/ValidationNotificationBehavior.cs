using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;
using ECommerce.Mediator.Shared.Enums;

namespace ECommerce.Mediator.Pipeline
{
  public sealed class ValidationNotificationBehavior<TCommand> : ICommandBehavior<TCommand, ResultNotification> where TCommand : ICommand<ResultNotification>
  {
    private readonly IEnumerable<ICommandNotificationValidator<TCommand>> _validators;
    private readonly INotificationContext _notification;

    public ValidationNotificationBehavior(IEnumerable<ICommandNotificationValidator<TCommand>> validators, INotificationContext notification)
    {
      _validators = validators;
      _notification = notification;
    }

    public async Task<ResultNotification> Handle(TCommand command, CancellationToken cancellationToken, Func<Task<ResultNotification>> next)
    {
      foreach (var validator in _validators)
      {
        var notifications = validator.Validate(command);
        foreach (var n in notifications)
        {
          if (n.Type == ENotificationType.Error)
            _notification.AddError(n.Message);
          else
            _notification.AddWarning(n.Message);
        }
      }

      if (_notification.HasErrors)
        return ResultNotification.Fail(_notification);

      return await next();
    }
  }
}
