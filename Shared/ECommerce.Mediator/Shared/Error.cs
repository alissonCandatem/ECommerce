namespace ECommerce.Mediator.Shared
{
  public sealed class Error
  {
    public string? Code { get; private init; }
    public string Message { get; private init; } = null!;

    public static Error NotFound(string message) =>
        new() { Code = "NotFound", Message = message };

    public static Error Conflict(string message) =>
        new() { Code = "Conflict", Message = message };

    public static Error Unauthorized(string message) =>
        new() { Code = "Unauthorized", Message = message };

    public static Error Forbidden(string message) =>
        new() { Code = "Forbidden", Message = message };

    public static Error BadRequest(string message) =>
        new() { Code = "BadRequest", Message = message };
  }
}
