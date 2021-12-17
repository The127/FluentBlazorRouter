using FluentBlazorRouter.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace FluentBlazorRouter;

public static class ServiceExtensions
{
    public static IServiceCollection AddFluentRouting<TRootPage>(
        this IServiceCollection serviceCollection,
        Action<RouteGroupBuilder> rootGroupBuilderConfigurationAction,
        Action<FluentRouterConfigurationOptionsBuilder>? configurationAction = null)
        where TRootPage : IComponent =>
        AddFluentRouting(serviceCollection, rootGroupBuilderConfigurationAction, configurationAction,
            typeof(TRootPage));


    public static IServiceCollection AddFluentRouting(
        this IServiceCollection serviceCollection,
        Action<RouteGroupBuilder> rootGroupBuilderConfigurationAction,
        Action<FluentRouterConfigurationOptionsBuilder>? configurationAction = null) =>
        AddFluentRouting(serviceCollection, rootGroupBuilderConfigurationAction, configurationAction, null);

    private static IServiceCollection AddFluentRouting(
        this IServiceCollection serviceCollection,
        Action<RouteGroupBuilder> rootGroupBuilderConfigurationAction,
        Action<FluentRouterConfigurationOptionsBuilder>? configurationAction,
        Type? rootPageType)
    {
        
        FluentRouterConfigurationOptionsBuilder configurationOptionsBuilder = new();
        configurationAction?.Invoke(configurationOptionsBuilder);
        var fluentRouterOptions = configurationOptionsBuilder.BuildConfiguration();
        
        var rootGroupBuilder = new RouteGroupBuilder("", rootPageType, new RouteMatcherCompiler(fluentRouterOptions));
        rootGroupBuilderConfigurationAction.Invoke(rootGroupBuilder);

        serviceCollection.AddSingleton(_ => fluentRouterOptions);
        serviceCollection.AddSingleton<IRouteProvider>(new RouteProvider(rootGroupBuilder));
        serviceCollection.AddSingleton<RouteMatcherCompiler>();

        return serviceCollection;
    }
}