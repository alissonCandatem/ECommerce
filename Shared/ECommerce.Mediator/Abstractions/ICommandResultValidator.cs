using ECommerce.Mediator.Shared;

namespace ECommerce.Mediator.Abstractions
{
  public interface ICommandResultValidator<TCommand>
  {
    Error? Validate(TCommand command);
  }
}
