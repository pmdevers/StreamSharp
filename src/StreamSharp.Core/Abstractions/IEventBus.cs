using Microsoft.Extensions.DependencyInjection;

namespace StreamSharp.Core.Abstractions;

public interface IEventBus
{
    ValueTask PublishAsync<T>(T message, CancellationToken ct = default)
        where T : DomainEvent;
}

public static class EventBusExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMessageHandler<T>(DomainEventHandler<T> handler)
            where T : DomainEvent
        {
            services.AddSingleton(handler);
            return services;
        }

        public IServiceCollection AddMessageHandler<THandler>()
            where THandler : class
        {
            var handlerType = typeof(THandler);

            var methods = handlerType
                .GetMethods()
                .Where(m =>
                {
                    if (m.ReturnType != typeof(Task))
                        return false;
                    var parameters = m.GetParameters();
                    return parameters.Length == 2
                        && typeof(DomainEvent).IsAssignableFrom(parameters[0].ParameterType)
                        && parameters[1].ParameterType == typeof(CancellationToken);
                })
                .ToArray();

            if (methods.Length == 0)
                throw new InvalidOperationException(
                    $"{handlerType.Name} has no methods matching the DomainEventHandler delegate signature " +
                    $"'Task MethodName(T message, CancellationToken ct) where T : DomainEvent'.");

            services.AddTransient<THandler>();

            foreach (var method in methods)
            {
                var eventType = method.GetParameters()[0].ParameterType;
                var delegateType = typeof(DomainEventHandler<>).MakeGenericType(eventType);

                services.AddTransient(delegateType, sp =>
                {
                    var instance = sp.GetRequiredService<THandler>();
                    return Delegate.CreateDelegate(delegateType, instance, method);
                });
            }

            return services;
        }
    }


    public static async ValueTask PublishAsync<T>(this IEventBus bus, IEnumerable<T> messages, CancellationToken ct = default)
        where T : DomainEvent
    {

        foreach (var message in messages)
        {
            await bus.PublishAsync(message, ct);
        }
    }
}

public delegate Task DomainEventHandler<in T>(T @event, CancellationToken ct = default)
    where T : DomainEvent;
