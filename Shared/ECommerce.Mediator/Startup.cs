using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Pipeline;
using ECommerce.Mediator.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Mediator
{
  public static class Startup
  {
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
      services.AddScoped<ICommandDispatcher, CommandDispatcher>();
      services.AddScoped<INotificationContext, NotificationContext>();
      return services;
    }
  }
}
