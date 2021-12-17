using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace FluentBlazorRouter.Internal;

public sealed class RouteProvider : IRouteProvider
{
    private IReadOnlyCollection<Route> _routes;

    public RouteProvider(RouteGroupBuilder rootGroupBuilder)
    {
        _routes = rootGroupBuilder.BuildRoutes();
    }
    
    public bool TryMatch(string relativeUri, out Dictionary<string, object> routeValues, [NotNullWhen(true)] out Type? pageType)
    {
        pageType = null;
        routeValues = new Dictionary<string, object>();
        
        foreach (var route in _routes)
        {
            if (!route.Matches(relativeUri, routeValues)) continue;
            
            pageType = route.PageType;
            return true;
        }
        
        return false;
    }

    public bool TryGetPageRoute<TPage>([NotNullWhen(true)] out string? route) 
        where TPage : IComponent => 
        TryGetPageRoute(typeof(TPage), out route);

    public bool TryGetPageRoute(Type pageType, [NotNullWhen(true)] out string? route)
    {
        foreach (var routeVar in _routes)
        {
            if (routeVar.PageType == pageType)
            {
                route = routeVar.FullRoute;
            }
        }

        route = null;
        return false;
    }
}