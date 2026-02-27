using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;

namespace ECommerce.Mediator.Pipeline
{
  public sealed class TransactionBehavior<TCommand, TResult> : ICommandBehavior<TCommand, TResult> where TCommand : ICommand<TResult>
  {
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContext _notification;

    public TransactionBehavior(
      IUnitOfWork unitOfWork,
      INotificationContext notification)
    {
      _unitOfWork = unitOfWork;
      _notification = notification;
    }

    public async Task<TResult> Handle(TCommand command, CancellationToken cancellationToken, Func<Task<TResult>> next)
    {
      await _unitOfWork.Begin();
      var result = await next();

      if (result is Result baseResult)
      {
        Console.WriteLine($"IsSuccess: {baseResult.IsSuccess}");
        Console.WriteLine($"Error: {baseResult.Error?.Message}");
      }

      var failed = result switch
      {
        ResultNotification r => r.IsSuccess == false,
        Result r => r.IsSuccess == false,
        _ => _notification.HasErrors
      };

      Console.WriteLine($"Result type: {result?.GetType().Name}");
      Console.WriteLine($"Failed: {failed}");

      if (failed || _notification.HasErrors)
      {
        Console.WriteLine("ROLLBACK");
        await _unitOfWork.Rollback();
        return result;
      }

      Console.WriteLine("COMMIT");
      await _unitOfWork.Commit();
      return result;
    }
  }
}
