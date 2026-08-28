using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace FluentBlazorRouter.Internal;

public sealed class RouteProvider : IRouteProvider
{
    private IReadOnlyCollection<Route> _routes;

    public RouteProvider(RouteGroupBuilder rootGroupBuilder)
    {
        _routes = rootGroupBuilder.BuildRoutes();
        ValidateRoutes();
    }

    private void ValidateRoutes()
    {
        foreach (var route in _routes)
        {
            route.Validate();
        }
    }

    public bool TryMatch(string relativeUri, out Dictionary<string, object?> routeValues, [NotNullWhen(true)] out Route? route)
    {
        routeValues = new Dictionary<string, object?>();

        foreach (var candidate in _routes)
        {
            if (!candidate.Matches(relativeUri, routeValues)) continue;

            route = candidate;
            return true;
        }

        route = null;
        return false;
    }

    public bool TryGetRouteData(Type pageType, [NotNullWhen(true)] out Route? route)
    {
        route = _routes.FirstOrDefault(x => x.PageType == pageType);
        return route is not null;
    }

    public bool TryGetPageRoute(Type pageType, [NotNullWhen(true)] out string? route)
    {
        foreach (var routeVar in _routes)
        {
            if (routeVar.PageType == pageType)
            {
                route = routeVar.FullRoute;
                return true;
            }
        }

        route = null;
        return false;
    }
}