using Shouldly;
using Xunit;

namespace FluentBlazorRouter.UnitTests;

public class BuiltinSegmentMatcherTests
{
    private static ISegmentMatcher Matcher(string key) =>
        new FluentRouterConfigurationOptionsBuilder().BuildConfiguration().SegmentMatchers[key];

    [Theory]
    [InlineData("string", typeof(string))]
    [InlineData("byte", typeof(byte))]
    [InlineData("short", typeof(short))]
    [InlineData("int", typeof(int))]
    [InlineData("long", typeof(long))]
    [InlineData("float", typeof(float))]
    [InlineData("double", typeof(double))]
    [InlineData("guid", typeof(Guid))]
    public void BuiltinMatchers_ExposeTheirMatchType(string key, Type expected)
        => Matcher(key).MatchType.ShouldBe(expected);

    [Theory]
    [InlineData("string", "anything")]
    [InlineData("byte", "255")]
    [InlineData("short", "-32768")]
    [InlineData("int", "42")]
    [InlineData("long", "9223372036854775807")]
    [InlineData("float", "1.5")]
    [InlineData("double", "1.5")]
    [InlineData("guid", "8a1f3d2e-0000-0000-0000-000000000000")]
    public void BuiltinMatchers_AcceptValidSegments(string key, string segment)
        => Matcher(key).MatchSegment(segment, out _).ShouldBeTrue();

    [Theory]
    [InlineData("byte", "256")]
    [InlineData("short", "40000")]
    [InlineData("int", "notanumber")]
    [InlineData("long", "1.5")]
    [InlineData("double", "notanumber")]
    [InlineData("guid", "notaguid")]
    public void BuiltinMatchers_RejectInvalidSegments(string key, string segment)
        => Matcher(key).MatchSegment(segment, out _).ShouldBeFalse();

    [Fact]
    public void StringMatcher_AcceptsEmptySegment()
        => Matcher("string").MatchSegment(string.Empty, out var value).ShouldBeTrue();

    [Fact]
    public void IntMatcher_YieldsTheParsedValue()
    {
        Matcher("int").MatchSegment("42", out var value).ShouldBeTrue();
        value.ShouldBe(42);
    }
}
