using System.Text.RegularExpressions;

namespace FluentBlazorRouter.Internal;

internal static class SegmentSyntax
{
    internal const string Identifier = @"[\p{L}\p{Nl}_][\p{L}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}]*";

    private static readonly Regex IdentifierRegex = new($@"\A{Identifier}\z");

    internal static bool IsIdentifier(string? value) => value is not null && IdentifierRegex.IsMatch(value);
}
