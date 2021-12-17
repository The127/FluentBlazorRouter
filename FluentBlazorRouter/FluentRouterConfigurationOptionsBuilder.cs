using FluentBlazorRouter.Internal;

namespace FluentBlazorRouter;

public sealed class FluentRouterConfigurationOptionsBuilder
{
    private readonly Dictionary<string, ISegmentMatcher> _segmentMatchers = new()
    {
        {"string", new StringSegmentMatcher()},
        {"byte", new ByteSegmentMatcher()},
        {"short", new ShortSegmentMatcher()},
        {"int", new IntSegmentMatcher()},
        {"long", new LongSegmentMatcher()},
        {"float", new FloatSegmentMatcher()},
        {"double", new DoubleSegmentMatcher()},
        {"guid", new GuidSegmentMatcher()},
    };

    public void AddSegmentMatcher(string segmentIdentifier, ISegmentMatcher segmentMatcher) =>
        _segmentMatchers[segmentIdentifier] = segmentMatcher;

    internal FluentRouterOptions BuildConfiguration() =>
        new FluentRouterOptions(new Dictionary<string, ISegmentMatcher>(_segmentMatchers));
}