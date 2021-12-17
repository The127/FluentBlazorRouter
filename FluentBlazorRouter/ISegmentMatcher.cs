namespace FluentBlazorRouter;

public interface ISegmentMatcher
{
    bool MatchSegment(string segment, out object segmentValue);
}