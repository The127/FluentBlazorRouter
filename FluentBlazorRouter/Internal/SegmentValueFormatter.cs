using System.Globalization;

namespace FluentBlazorRouter.Internal;

internal static class SegmentValueFormatter
{
    internal static string Format(object? value, Type? matchType)
    {
        if (value is null)
        {
            throw new InvalidOperationException($"Cannot format a null route value for match type '{matchType?.FullName}'. Override FormatValue to support it.");
        }

        var expected = matchType is null ? null : Nullable.GetUnderlyingType(matchType) ?? matchType;
        if (expected is not null && !expected.IsInstanceOfType(value))
        {
            throw new InvalidOperationException($"Route value of type '{value.GetType().FullName}' does not match the expected type '{expected.FullName}'.");
        }

        var formatted = (value is IFormattable formattable
                            ? formattable.ToString(null, CultureInfo.InvariantCulture)
                            : value.ToString())
                        ?? throw new InvalidOperationException($"Route value of type '{value.GetType().FullName}' has a null string representation. Override FormatValue to support it.");

        if (formatted.Contains('/'))
        {
            throw new InvalidOperationException($"Route value '{formatted}' contains a path separator and cannot be used as a single segment.");
        }

        return formatted;
    }
}
