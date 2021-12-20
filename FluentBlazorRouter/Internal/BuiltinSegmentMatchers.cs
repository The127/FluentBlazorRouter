namespace FluentBlazorRouter.Internal;

internal class StringSegmentMatcher : SegmentMatcherBase<string>
{
    public override bool MatchSegment(string segment, out object segmentValue)
    {
        segmentValue = segment;
        return true;
    }
}

internal class ByteSegmentMatcher : SegmentMatcherBase<byte>
{
    public override bool MatchSegment(string segment, out object segmentValue)
    {
        if (byte.TryParse(segment, out var value))
        {
            segmentValue = value;
            return true;
        }

        segmentValue = 0;
        return false;
    }
}

internal class ShortSegmentMatcher : SegmentMatcherBase<short>
{
    public override bool MatchSegment(string segment, out object segmentValue)
    {
        if (short.TryParse(segment, out var value))
        {
            segmentValue = value;
            return true;
        }

        segmentValue = (short) 0;
        return false;
    }
}

internal class IntSegmentMatcher : SegmentMatcherBase<int>
{
    public override bool MatchSegment(string segment, out object segmentValue)
    {
        if (int.TryParse(segment, out var value))
        {
            segmentValue = value;
            return true;
        }

        segmentValue = 0;
        return false;
    }
}

internal class LongSegmentMatcher : SegmentMatcherBase<long>
{
    public override bool MatchSegment(string segment, out object segmentValue)
    {
        if (long.TryParse(segment, out var value))
        {
            segmentValue = value;
            return true;
        }

        segmentValue = 0L;
        return false;
    }
}

internal class FloatSegmentMatcher : SegmentMatcherBase<float>
{
    public override bool MatchSegment(string segment, out object segmentValue)
    {
        if (float.TryParse(segment, out var value))
        {
            segmentValue = value;
            return true;
        }

        segmentValue = 0.0f;
        return false;
    }
}

internal class DoubleSegmentMatcher : SegmentMatcherBase<double>
{
    public override bool MatchSegment(string segment, out object segmentValue)
    {
        if (double.TryParse(segment, out var value))
        {
            segmentValue = value;
            return true;
        }

        segmentValue = 0.0d;
        return false;
    }
}

internal class GuidSegmentMatcher : SegmentMatcherBase<Guid>
{
    public override bool MatchSegment(string segment, out object segmentValue)
    {
        if (Guid.TryParse(segment, out var value))
        {
            segmentValue = value;
            return true;
        }

        segmentValue = Guid.Empty;
        return false;
    }
}
