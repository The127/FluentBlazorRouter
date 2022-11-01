using FluentBlazorRouter.Internal;
using Microsoft.AspNetCore.Components;

namespace FluentBlazorRouter;

public class RouteGroupBuilder
{
    private readonly Type? _pageType;
    private readonly RouteMatcherCompiler _routeMatcherCompiler;
    private readonly string _pathSegment;

    private readonly List<RouteGroupBuilder> _subGroupBuilders = new();
    private Dictionary<Type, object> _metadata = new();

    internal RouteGroupBuilder(string pathSegment, Type? pageType, RouteMatcherCompiler routeMatcherCompiler)
    {
        _pageType = pageType;
        _routeMatcherCompiler = routeMatcherCompiler;
        _pathSegment = pathSegment;
    }

    public RouteGroupBuilder WithPage<TPage>(string pathSegment, Action<RouteGroupBuilder>? subGroupConfigurationAction = null)
        where TPage : IComponent =>
        WithSubRoute(typeof(TPage), pathSegment, subGroupConfigurationAction);

    public RouteGroupBuilder WithGroup(string groupPathSegment, Action<RouteGroupBuilder> subGroupConfigurationAction) =>
        WithSubRoute(null, groupPathSegment, subGroupConfigurationAction);

    private RouteGroupBuilder WithSubRoute(Type? pageType, string pagePathSegment, Action<RouteGroupBuilder>? subGroupConfigurationAction)
    {
        var subRouteGroupBuilder = new RouteGroupBuilder(pagePathSegment.Trim('/'), pageType, _routeMatcherCompiler);
        _subGroupBuilders.Add(subRouteGroupBuilder);
        
        subGroupConfigurationAction?.Invoke(subRouteGroupBuilder);
        return this;
    }

    private void BuildRoutes(string parentPath, List<Route> routeList, Route? parent)
    {
        var thisPath = parentPath + "/" + _pathSegment;
        
        var route = parent;
        if (_pageType is not null)
        {
            route = new Route(thisPath.TrimStart('/'), _pageType, _routeMatcherCompiler, _metadata, parent);
            routeList.Add(route);
        }

        foreach (var subRouteGroupBuilder in _subGroupBuilders)
        {
            subRouteGroupBuilder.BuildRoutes(thisPath.TrimStart('/'), routeList, route);
        }
    }
    
    public RouteGroupBuilder WithMetadata<T>(T metadata)
        where T : notnull
    {
        _metadata.Add(typeof(T), metadata);
        return this;
    }

    internal IReadOnlyCollection<Route> BuildRoutes()
    {
        var result = new List<Route>();
        BuildRoutes("", result, null);
        return result;
    }
}