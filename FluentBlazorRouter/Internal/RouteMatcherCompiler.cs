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
            if (segment.StartsWith("{")) {
                var match = Regex.Match(segment, $"^{{(?<parameter>{SegmentSyntax.Identifier})(?::(?<type>{SegmentSyntax.Identifier}))?}}$");
                if (!match.Success)
                {
                    throw new Exception($"Route segment error in '{fullRoute}' at '{segment}'.");
                }
                
                // Check the segment is {Parameter:Type}
                if (match.Groups["type"].Length > 0) {

                    var segmentMatcherKey = match.Groups["type"].Value;
                    var segmentPropertyName = match.Groups["parameter"].Value;

                    if (!_fluentRouterOptions.SegmentMatchers.TryGetValue(segmentMatcherKey, out var matcher))
                    {
                        throw new Exception($"No matcher registered for key '{segmentMatcherKey}'.");
                    }
                    
                    segmentMatchers.Add(new SegmentMatcherHandler(matcher, segmentPropertyName));
                }
                // default to string if no type was provided
                else
                {
                    segmentMatchers.Add(new SegmentMatcherHandler(_fluentRouterOptions.SegmentMatchers["string"], match.Groups["parameter"].Value));
                }
            }
            else
            {
                // special case => no matcher, the strings have to match
                segmentMatchers.Add(new SegmentMatcherHandler(null, Uri.UnescapeDataString(segment)));
            }
        }

        return new RouteMatcher(segmentMatchers);
    }
}