namespace FluentBlazorRouter.Internal;

internal class StringSegmentMatcher : ISegmentMatcher
{
    public bool MatchSegment(string segment, out object segmentValue)
    {
        segmentValue = segment;
        return true;
    }
}

internal class ByteSegmentMatcher : ISegmentMatcher
{
    public bool MatchSegment(string segment, out object segmentValue)
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

internal class ShortSegmentMatcher : ISegmentMatcher
{
    public bool MatchSegment(string segment, out object segmentValue)
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

internal class IntSegmentMatcher : ISegmentMatcher
{
    public bool MatchSegment(string segment, out object segmentValue)
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

internal class LongSegmentMatcher : ISegmentMatcher
{
    public bool MatchSegment(string segment, out object segmentValue)
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

internal class FloatSegmentMatcher : ISegmentMatcher
{
    public bool MatchSegment(string segment, out object segmentValue)
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

internal class DoubleSegmentMatcher : ISegmentMatcher
{
    public bool MatchSegment(string segment, out object segmentValue)
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

internal class GuidSegmentMatcher : ISegmentMatcher
{
    public bool MatchSegment(string segment, out object segmentValue)
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
