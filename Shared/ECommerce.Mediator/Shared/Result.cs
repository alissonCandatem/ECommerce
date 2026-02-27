namespace ECommerce.Mediator.Shared
{
  public class Result
  {
    public bool IsSuccess { get; }
    public Error? Error { get; }

    protected Result(bool success, Error? error)
    {
      IsSuccess = success;
      Error = error;
    }

    public static Result Ok()
        => new(true, null);

    public static Result Fail(Error error)
        => new(false, error);
  }
}
