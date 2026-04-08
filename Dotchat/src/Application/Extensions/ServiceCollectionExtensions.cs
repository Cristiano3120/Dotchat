using DotchatServer.src.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace DotchatServer.src.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKeyedSingletonWithWarmup<TInterface, TImplementation>(this IServiceCollection services, object key)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        _ = services.AddKeyedSingleton<TInterface, TImplementation>(key);
        _ = services.AddSingleton<IWarmable>(sp => (sp.GetRequiredKeyedService<TInterface>(key) as IWarmable)!);
        
        return services;
    }

    public static IServiceCollection AddSingletonWithWarmup<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        _ = services.AddSingleton<TInterface, TImplementation>();
        _ = services.AddSingleton<IWarmable>(sp => (sp.GetRequiredService<TInterface>() as IWarmable)!);

        return services;
    }

    public static IServiceCollection AddDbContextPoolWithWarmup<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TContext, WarmupUtility>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction, int poolSize = 1024) where TContext : DbContext
        where WarmupUtility : class, IWarmable
    {
        _ = services.AddDbContextPool<TContext>(optionsAction, poolSize);
        _ = services.AddSingleton<IWarmable, WarmupUtility>();

        return services;
    }
}