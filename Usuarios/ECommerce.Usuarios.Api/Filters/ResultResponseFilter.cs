using ECommerce.Mediator.Shared;
using ECommerce.Usuarios.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.Usuarios.Filters
{
  public sealed class ResultResponseFilter : IAsyncResultFilter
  {
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
      if (context.Result is ObjectResult objectResult)
      {
        var value = objectResult.Value;

        var resultResponse = value switch
        {
          Result r => ResultResponse.From(r),
          ResultNotification r => ResultResponse.From(r),
          _ => null
        };

        if (resultResponse != null)
          context.Result = resultResponse;
      }

      await next();
    }
  }
}
