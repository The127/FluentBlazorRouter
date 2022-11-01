using System.Diagnostics.CodeAnalysis;
using FluentBlazorRouter.Internal;
using Microsoft.AspNetCore.Components;

namespace FluentBlazorRouter;

public interface IRouteProvider
{
    bool TryGetPageRoute<TPage>([NotNullWhen(true)] out string? route)
        where TPage : IComponent
        => TryGetPageRoute(typeof(TPage), out route);

    bool TryGetPageRoute(Type pageType, [NotNullWhen(true)] out string? route);

    bool TryMatch(string relativeUri, out Dictionary<string, object> routeValues,
        [NotNullWhen(true)] out Type? pageType);

    bool TryGetRouteData<TPage>([NotNullWhen(true)] out Route? route) => TryGetRouteData(typeof(TPage), out route);
    bool TryGetRouteData(Type pageType, [NotNullWhen(true)] out Route? route);
}

public record RouteChangedEventArgs(RouteData RouteData);