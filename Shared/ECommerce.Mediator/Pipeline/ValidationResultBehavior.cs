using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;

namespace ECommerce.Mediator.Pipeline
{
  public sealed class ValidationResultBehavior<TCommand> : ICommandBehavior<TCommand, Result> where TCommand : ICommand<Result>
  {
    private readonly IEnumerable<ICommandResultValidator<TCommand>> _validators;

    public ValidationResultBehavior(IEnumerable<ICommandResultValidator<TCommand>> validators)
    {
      _validators = validators;
    }

    public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken, Func<Task<Result>> next)
    {
      foreach (var validator in _validators)
      {
        var error = validator.Validate(command);

        if (error != null)
          return Result.Fail(error);
      }

      return await next();
    }
  }
}
