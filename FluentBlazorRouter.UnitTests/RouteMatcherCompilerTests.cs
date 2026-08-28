using FluentBlazorRouter.Internal;
using Shouldly;
using Xunit;

namespace FluentBlazorRouter.UnitTests;

public class RouteMatcherCompilerTests
{
    private static RouteMatcherCompiler CreateCompiler() =>
        new(new FluentRouterConfigurationOptionsBuilder().BuildConfiguration());

    [Fact]
    public void Compile_StaticRoute_MatchesThatRouteOnly()
    {
        var matcher = CreateCompiler().Compile("group/example");

        matcher.Matches("group/example", new Dictionary<string, object>()).ShouldBeTrue();
        matcher.Matches("group/other", new Dictionary<string, object>()).ShouldBeFalse();
    }

    [Fact]
    public void Compile_UntypedParameter_DefaultsToStringMatcher()
    {
        var matcher = CreateCompiler().Compile("page/{ActiveTab}");
        var routeValues = new Dictionary<string, object>();

        matcher.Matches("page/anything", routeValues).ShouldBeTrue();
        routeValues.ShouldContainKeyAndValue("ActiveTab", "anything");
    }

    [Fact]
    public void Compile_TypedParameter_UsesTheRegisteredMatcher()
    {
        var matcher = CreateCompiler().Compile("counter/{Id:int}");
        var routeValues = new Dictionary<string, object>();

        matcher.Matches("counter/42", routeValues).ShouldBeTrue();
        routeValues["Id"].ShouldBe(42);
    }

    [Theory]
    [InlineData("{Id:}")]
    [InlineData("{}")]
    [InlineData("{Id:int:extra}")]
    [InlineData("{Id")]
    public void Compile_MalformedParameterSegment_Throws(string segment)
    {
        var exception = Should.Throw<Exception>(() => CreateCompiler().Compile(segment));

        exception.Message.ShouldContain("Route segment error");
    }

    [Theory]
    [InlineData("{Id1}")]
    [InlineData("{Page2}")]
    [InlineData("{my_id}")]
    public void Compile_ParameterNameWithDigitOrUnderscore_ThrowsToday(string segment)
    {
        var exception = Should.Throw<Exception>(() => CreateCompiler().Compile(segment));

        exception.Message.ShouldContain("Route segment error");
    }

    [Fact]
    public void Compile_UnknownSegmentType_Throws()
    {
        var exception = Should.Throw<Exception>(() => CreateCompiler().Compile("counter/{Id:notatype}"));

        exception.Message.ShouldContain("No matcher registered for key 'notatype'");
    }

    [Fact]
    public void Compile_CustomRegisteredMatcher_IsUsed()
    {
        var builder = new FluentRouterConfigurationOptionsBuilder();
        builder.AddSegmentMatcher("even", new EvenNumberMatcher());
        var matcher = new RouteMatcherCompiler(builder.BuildConfiguration()).Compile("n/{Value:even}");

        var routeValues = new Dictionary<string, object>();
        matcher.Matches("n/4", routeValues).ShouldBeTrue();
        routeValues["Value"].ShouldBe(4);

        matcher.Matches("n/5", new Dictionary<string, object>()).ShouldBeFalse();
    }

    private sealed class EvenNumberMatcher : SegmentMatcherBase<int>
    {
        public override bool MatchSegment(string segment, out object segmentValue)
        {
            segmentValue = 0;
            if (!int.TryParse(segment, out var value) || value % 2 != 0)
            {
                return false;
            }

            segmentValue = value;
            return true;
        }
    }
}
