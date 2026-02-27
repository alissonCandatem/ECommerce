namespace ECommerce.Mediator.Abstractions
{
  public interface ICommandBehavior<TCommand, TResult> where TCommand : ICommand<TResult>
  {
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken, Func<Task<TResult>> next);
  }
}
