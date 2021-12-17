namespace FluentBlazorRouter.Internal;

internal class SegmentMatcherHandler
{
    internal ISegmentMatcher? Matcher { get; }
    internal string SegmentPropertyName { get; }

    public SegmentMatcherHandler(ISegmentMatcher? matcher, string segmentPropertyName)
    {
        Matcher = matcher;
        SegmentPropertyName = segmentPropertyName;
    }
}