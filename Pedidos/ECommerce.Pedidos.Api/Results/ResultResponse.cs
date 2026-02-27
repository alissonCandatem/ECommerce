using ECommerce.Mediator.Shared;
using ECommerce.Mediator.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Pedidos.Results
{
  public sealed class ResultResponse : IActionResult
  {
    private readonly object _value;
    private readonly int _statusCode;

    private ResultResponse(object value, int statusCode)
    {
      _value = value;
      _statusCode = statusCode;
    }

    public static ResultResponse From(Result result)
    {
      if (result.IsSuccess)
      {
        var resultType = result.GetType();

        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
          var value = resultType.GetProperty("Value")!.GetValue(result);
          return new(value!, 200);
        }

        return new(result, 200);
      }

      var statusCode = result.Error?.Code switch
      {
        "NotFound" => 404,
        "Forbidden" => 403,
        "Conflict" => 409,
        "Unauthorized" => 401,
        _ => 400
      };

      return new(result, statusCode);
    }

    public static ResultResponse From(ResultNotification result)
    {
      if (result.IsSuccess)
        return new(result, 200);

      var firstError = result.Notifications
          .FirstOrDefault(n => n.Type == ENotificationType.Error);

      var statusCode = firstError?.Code switch
      {
        "NotFound" => 404,
        "Forbidden" => 403,
        "Conflict" => 409,
        "Unauthorized" => 401,
        _ => 400
      };

      return new(result, statusCode);
    }

    public Task ExecuteResultAsync(ActionContext context)
    {
      var objectResult = new ObjectResult(_value)
      {
        StatusCode = _statusCode
      };
      return objectResult.ExecuteResultAsync(context);
    }
  }
}
