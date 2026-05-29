using System.Diagnostics.CodeAnalysis;
using DotchatServer.src.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DotchatServer.src.Application.Extensions;

/// <summary>
/// Adds extension methods for registering services with warmup capabilities in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a keyed singleton mapping from TInterface to TImplementation and registers an IWarmable that resolves the
    /// keyed instance for warm-up.
    /// </summary>
    /// <remarks>Registers a keyed singleton for TInterface and a singleton IWarmable that resolves the keyed
    /// instance. Resolution will fail if the keyed service is not registered or does not implement IWarmable.</remarks>
    /// <typeparam name="TInterface">The service interface type.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type that implements TInterface.</typeparam>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="key">The key used to register and resolve the keyed service.</param>
    /// <returns>The original IServiceCollection for chaining.</returns>
    public static IServiceCollection AddKeyedSingletonWithWarmup<TInterface, TImplementation>(this IServiceCollection services, object key)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        _ = services.AddKeyedSingleton<TInterface, TImplementation>(key);
        _ = services.AddSingleton<IWarmable>(sp => (sp.GetRequiredKeyedService<TInterface>(key) as IWarmable)!);

        return services;
    }

    /// <summary>
    /// Registers TImplementation as a singleton for TInterface and also registers the same instance as an IWarmable to
    /// support warmup.
    /// </summary>
    /// <remarks>TImplementation must implement IWarmable for the IWarmable registration to succeed. The
    /// IWarmable registration resolves the same singleton instance as the TInterface registration.</remarks>
    /// <typeparam name="TInterface">The service contract interface type to register as a singleton.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type to register as a singleton; must implement TInterface.</typeparam>
    /// <param name="services">The IServiceCollection to which the registrations will be added.</param>
    /// <returns>The original IServiceCollection for chaining.</returns>
    public static IServiceCollection AddSingletonWithWarmup<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        _ = services.AddSingleton<TInterface, TImplementation>();
        _ = services.AddSingleton<IWarmable>(sp => (sp.GetRequiredService<TInterface>() as IWarmable)!);

        return services;
    }

    /// <summary>
    /// Adds TImplementation as a singleton and registers the same singleton instance as IWarmable.
    /// </summary>
    /// <remarks>Registers TImplementation as a singleton and registers IWarmable to resolve to the same
    /// instance by casting the singleton. The cast is unchecked and will throw at runtime if TImplementation does not
    /// implement IWarmable.</remarks>
    /// <typeparam name="TImplementation">The concrete implementation type to register as a singleton.</typeparam>
    /// <param name="services">The service collection to which the registrations are added.</param>
    /// <returns>The original IServiceCollection for call chaining.</returns>
    public static IServiceCollection AddSingletonWithWarmup<TImplementation>(this IServiceCollection services)
        where TImplementation : class
    {
        _ = services.AddSingleton<TImplementation>();
        _ = services.AddSingleton<IWarmable>(sp => (sp.GetRequiredService<TImplementation>() as IWarmable)!);

        return services;
    }

    /// <summary>
    /// Adds a pooled DbContext of type TContext and registers WarmupUtility as a singleton IWarmable.
    /// </summary>
    /// <remarks>Calls AddDbContextPool<TContext> with the provided options and poolSize, and registers
    /// WarmupUtility as a singleton IWarmable.</remarks>
    /// <typeparam name="TContext">The DbContext type to add to the pool.</typeparam>
    /// <typeparam name="WarmupUtility">The IWarmable implementation used to perform warm-up operations.</typeparam>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="optionsAction">An action to configure DbContextOptionsBuilder for TContext.</param>
    /// <param name="poolSize">The maximum size of the DbContext pool. Defaults to 1024.</param>
    /// <returns>The original IServiceCollection to allow call chaining.</returns>
    public static IServiceCollection AddDbContextPoolWithWarmup<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TContext, WarmupUtility>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction, int poolSize = 1024) where TContext : DbContext
        where WarmupUtility : class, IWarmable
    {
        _ = services.AddDbContextPool<TContext>(optionsAction, poolSize);
        _ = services.AddSingleton<IWarmable, WarmupUtility>();

        return services;
    }
}