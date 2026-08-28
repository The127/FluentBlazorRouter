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

    public void AddSegmentMatcher(string segmentIdentifier, ISegmentMatcher segmentMatcher)
    {
        if (segmentIdentifier is null)
        {
            throw new ArgumentNullException(nameof(segmentIdentifier));
        }

        if (!SegmentSyntax.IsIdentifier(segmentIdentifier))
        {
            throw new ArgumentException(
                $"Segment matcher key '{segmentIdentifier}' cannot be used in a route. Keys must start with a letter or underscore and contain only letters, digits and underscores.",
                nameof(segmentIdentifier));
        }

        _segmentMatchers[segmentIdentifier] = segmentMatcher;
    }

    internal FluentRouterOptions BuildConfiguration() =>
        new FluentRouterOptions(new Dictionary<string, ISegmentMatcher>(_segmentMatchers));
}