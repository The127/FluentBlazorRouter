namespace FluentBlazorRouter.Internal;

internal sealed record Route
{
    public string FullRoute { get; init; }
    public Type PageType { get; init; }

    private readonly RouteMatcher _routeMatcher;
    
    public Route(string fullRoute, Type pageType, RouteMatcherCompiler compiler)
    {
        FullRoute = fullRoute;
        PageType = pageType;

        _routeMatcher = compiler.Compile(fullRoute);
    }

    public bool Matches(string relativeUri, Dictionary<string, object> routeValues) =>
        _routeMatcher.Matches(relativeUri, routeValues);
}