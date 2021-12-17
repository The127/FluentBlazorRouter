using FluentBlazorRouter.Internal;
using Microsoft.AspNetCore.Components;

namespace FluentBlazorRouter;

public class RouteGroupBuilder
{
    private readonly Type? _pageType;
    private readonly RouteMatcherCompiler _routeMatcherCompiler;
    private readonly string _pathSegment;

    private readonly List<RouteGroupBuilder> _subGroupBuilders = new();

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
        var subRouteGroupBuilder = new RouteGroupBuilder(pagePathSegment, pageType, _routeMatcherCompiler);
        _subGroupBuilders.Add(subRouteGroupBuilder);
        
        subGroupConfigurationAction?.Invoke(subRouteGroupBuilder);
        return this;
    }

    private void BuildRoutes(string parentPath, List<Route> routeList)
    {
        var thisPath = parentPath + "/" + _pathSegment;
        
        if (_pageType is not null)
        {
            routeList.Add(new Route(thisPath.TrimStart('/'), _pageType, _routeMatcherCompiler));
        }

        foreach (var subRouteGroupBuilder in _subGroupBuilders)
        {
            subRouteGroupBuilder.BuildRoutes(thisPath.TrimStart('/'), routeList);
        }
    }

    internal IReadOnlyCollection<Route> BuildRoutes()
    {
        var result = new List<Route>();
        BuildRoutes("", result);
        return result;
    }
}