namespace ECommerce.Mediator.Shared
{
  public sealed class Result<T> : Result
  {
    public T? Value { get; }

    private Result(bool success, T? value, Error? error) : base(success, error)
    {
      Value = value;
    }

    public static Result<T> Ok(T value)
        => new(true, value, null);

    public static Result<T> Fail(Error error)
        => new(false, default, error);
  }
}
