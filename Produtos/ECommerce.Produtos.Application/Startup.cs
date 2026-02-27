using ECommerce.Mediator;
using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Pipeline;
using ECommerce.Mediator.Shared;
using ECommerce.Produtos.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Produtos.Application
{
  public static class Startup
  {
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
      var assembly = typeof(IApplicationAssemblyMarker).Assembly;

      services.AddMediator();

      services.Scan(scan => scan
      .FromAssemblies(assembly)
      .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
      .AsImplementedInterfaces()
      .WithScopedLifetime());

      services.Scan(scan => scan
      .FromAssemblies(assembly)
      .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)))
      .AsImplementedInterfaces()
      .WithScopedLifetime());

      var commandHandlerInterfaces = assembly.GetTypes()
      .Where(t => !t.IsAbstract && !t.IsInterface)
      .SelectMany(t => t.GetInterfaces()
      .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)))
      .ToList();

      foreach (var handlerInterface in commandHandlerInterfaces)
      {
        var typeArgs = handlerInterface.GetGenericArguments();
        var commandType = typeArgs[0];
        var resultType = typeArgs[1];
        var behaviorInterface = typeof(ICommandBehavior<,>).MakeGenericType(commandType, resultType);

        if (resultType == typeof(ResultNotification))
        {
          services.AddScoped(behaviorInterface, typeof(NotificationResultBehavior<>).MakeGenericType(commandType));
          services.AddScoped(behaviorInterface, typeof(TransactionBehavior<,>).MakeGenericType(commandType, resultType));
          services.AddScoped(behaviorInterface, typeof(ValidationNotificationBehavior<>).MakeGenericType(commandType));
        }

        if (resultType == typeof(Result))
        {
          services.AddScoped(behaviorInterface, typeof(TransactionBehavior<,>).MakeGenericType(commandType, resultType));
          services.AddScoped(behaviorInterface, typeof(ValidationResultBehavior<>).MakeGenericType(commandType));
        }

        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
          services.AddScoped(behaviorInterface, typeof(TransactionBehavior<,>).MakeGenericType(commandType, resultType));
        }
      }

      return services;
    }
  }
}
