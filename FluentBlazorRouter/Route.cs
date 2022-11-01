using FluentBlazorRouter.Internal;

namespace FluentBlazorRouter;

public sealed record Route
{
    private readonly IReadOnlyDictionary<Type, object> _metadata;
    public Route? Parent { get; }
    public string FullRoute { get; }
    public Type PageType { get; }

    private readonly RouteMatcher _routeMatcher;
    
    internal Route(string fullRoute, Type pageType, RouteMatcherCompiler compiler, IReadOnlyDictionary<Type, object> metadata, Route? parent)
    {
        Parent = parent;
        _metadata = metadata;
        FullRoute = fullRoute;
        PageType = pageType;

        _routeMatcher = compiler.Compile(fullRoute);
    }

    public bool Matches(string relativeUri, Dictionary<string, object> routeValues) =>
        _routeMatcher.Matches(relativeUri, routeValues);

    public void Validate()
    {
        _routeMatcher.Validate(PageType);
    }
    
    public T GetMetadata<T>()
    {
        return (T)_metadata[typeof(T)];
    }
}