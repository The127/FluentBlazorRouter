using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace FluentBlazorRouter;

public interface IRouteProvider
{
    bool TryGetPageRoute<TPage>([NotNullWhen(true)] out string? route)
        where TPage : IComponent;

    bool TryGetPageRoute(Type pageType, [NotNullWhen(true)] out string? route);

    bool TryMatch(string relativeUri, out Dictionary<string, object> routeValues,
        [NotNullWhen(true)] out Type? pageType);
}