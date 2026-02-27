using ECommerce.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Mediator.Pipeline
{
  public sealed class CommandDispatcher : ICommandDispatcher
  {
    private readonly IServiceProvider _provider;

    public CommandDispatcher(IServiceProvider provider)
    {
      _provider = provider;
    }

    public async Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
      var commandType = command.GetType();

      // Handler
      var handlerType = typeof(ICommandHandler<,>)
          .MakeGenericType(commandType, typeof(TResult));
      var handler = _provider.GetRequiredService(handlerType);
      var handleMethod = handlerType.GetMethod("Handle")!;

      // Behaviors
      var behaviorType = typeof(ICommandBehavior<,>)
          .MakeGenericType(commandType, typeof(TResult));
      var behaviors = _provider.GetServices(behaviorType).Reverse().ToList();

      Func<Task<TResult>> next = () =>
          (Task<TResult>)handleMethod.Invoke(handler, [command, cancellationToken])!;

      foreach (var behavior in behaviors)
      {
        var behaviorMethod = behaviorType.GetMethod("Handle")!;
        var localNext = next;
        next = () => (Task<TResult>)behaviorMethod.Invoke(behavior, [command, cancellationToken, localNext])!;
      }

      return await next();
    }
  }
}
