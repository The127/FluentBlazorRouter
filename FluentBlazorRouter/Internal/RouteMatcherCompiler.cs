using System.Text.RegularExpressions;

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
                if (!Regex.IsMatch(segment, "{[a-zA-Z]+(:[a-zA-Z]+)?}"))
                {
                    throw new Exception($"Route segment error in '{fullRoute}' at '{segment}'.");
                }

                if (segment.Contains(':'))
                {
                    var parts = segment[1..^1].Split(":");

                    var segmentMatcherKey = parts[1];
                    var segmentPropertyName = parts[0];

                    if (!_fluentRouterOptions.SegmentMatchers.TryGetValue(segmentMatcherKey, out var matcher))
                    {
                        throw new Exception($"No matcher registered for key '{segmentMatcherKey}'.");
                    }
                    
                    segmentMatchers.Add(new SegmentMatcherHandler(matcher, segmentPropertyName));
                }
                // default to string if no type was provided
                else
                {
                    segmentMatchers.Add(new SegmentMatcherHandler(_fluentRouterOptions.SegmentMatchers["string"], segment));
                }
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