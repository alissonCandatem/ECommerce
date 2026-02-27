namespace ECommerce.Mediator.Abstractions
{
  public interface ICommandDispatcher
  {
    Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
  }
}
