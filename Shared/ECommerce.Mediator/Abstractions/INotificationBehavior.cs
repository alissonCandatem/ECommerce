using ECommerce.Mediator.Shared;

namespace ECommerce.Mediator.Abstractions
{
  public interface INotificationBehavior<TCommand> : ICommandBehavior<TCommand, ResultNotification> where TCommand : ICommand<ResultNotification>
  {
    Task<ResultNotification> Handle(TCommand command, CancellationToken cancellationToken, Func<Task<ResultNotification>> next);
  }
}
