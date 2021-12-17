namespace FluentBlazorRouter.Internal;

internal sealed class RouteMatcherCompiler
{
    private readonly FluentRouterOptions _fluentRouterOptions;

    public RouteMatcherCompiler(FluentRouterOptions fluentRouterOptions)
    {
        _fluentRouterOptions = fluentRouterOptions;
    }

    internal RouteMatcher Compile(string fullRoute)
    {
        var segmentMatchers = new List<SegmentMatcherHandler>();

        foreach (var segment in fullRoute.Split("/"))
        {
            if (segment.StartsWith("{"))
            {
                var parts = segment[1..^1].Split(":");

                var segmentMatcherKey = parts[0];
                var segmentPropertyName = parts[1];

                var matcher = _fluentRouterOptions.SegmentMatchers[segmentMatcherKey];
                segmentMatchers.Add(new SegmentMatcherHandler(matcher, segmentPropertyName));
            }
            else
            {
                // special case => no matcher, the strings have to match
                segmentMatchers.Add(new SegmentMatcherHandler(null, segment));
            }
        }

        return new RouteMatcher(segmentMatchers);
    }
}