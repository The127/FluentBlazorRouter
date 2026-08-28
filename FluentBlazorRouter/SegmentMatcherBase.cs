using FluentBlazorRouter.Internal;

namespace FluentBlazorRouter;

public abstract class SegmentMatcherBase<T> : ISegmentMatcher
{
    public abstract bool MatchSegment(string segment, out object? segmentValue);

    public Type MatchType => typeof(T);

    public virtual string FormatValue(object? value) => SegmentValueFormatter.Format(value, MatchType);
}