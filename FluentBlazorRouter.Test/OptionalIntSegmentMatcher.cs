using System.Globalization;

namespace FluentBlazorRouter.Test;

public class OptionalIntSegmentMatcher : SegmentMatcherBase<int?>
{
    private const string NoneSegment = "none";

    public override bool MatchSegment(string segment, out object? segmentValue)
    {
        segmentValue = null;

        if (segment == NoneSegment)
        {
            return true;
        }

        if (!int.TryParse(segment, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value))
        {
            return false;
        }

        segmentValue = value;
        return true;
    }

    public override string FormatValue(object? value) =>
        value is null ? NoneSegment : base.FormatValue(value);
}
