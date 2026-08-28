using FluentBlazorRouter.Internal;
using Shouldly;
using Xunit;

namespace FluentBlazorRouter.UnitTests;

public class RouteMatcherTests
{
    private static RouteMatcher Compile(string route) =>
        new RouteMatcherCompiler(new FluentRouterConfigurationOptionsBuilder().BuildConfiguration()).Compile(route);

    [Fact]
    public void Matches_DifferentSegmentCount_ReturnsFalse()
    {
        var matcher = Compile("counter/{Id:int}");

        matcher.Matches("counter", new Dictionary<string, object>()).ShouldBeFalse();
        matcher.Matches("counter/42/extra", new Dictionary<string, object>()).ShouldBeFalse();
    }

    [Fact]
    public void Matches_StaticSegmentDiffers_ReturnsFalse()
    {
        Compile("counter/{Id:int}")
            .Matches("other/42", new Dictionary<string, object>())
            .ShouldBeFalse();
    }

    [Fact]
    public void Matches_SegmentFailsToParse_ReturnsFalse()
    {
        Compile("counter/{Id:int}")
            .Matches("counter/notanumber", new Dictionary<string, object>())
            .ShouldBeFalse();
    }

    [Fact]
    public void Matches_MultipleParameters_CollectsEveryRouteValue()
    {
        var matcher = Compile("{Name}/{Id:int}/{Ratio:double}");
        var routeValues = new Dictionary<string, object>();

        matcher.Matches("thing/7/1.5", routeValues).ShouldBeTrue();

        routeValues.ShouldContainKeyAndValue("Name", "thing");
        routeValues.ShouldContainKeyAndValue("Id", 7);
        routeValues.ShouldContainKeyAndValue("Ratio", 1.5d);
    }

    [Fact]
    public void Matches_WhenALaterSegmentFails_LeavesRouteValuesUntouched()
    {
        var matcher = Compile("{Name}/{Id:int}");
        var routeValues = new Dictionary<string, object>();

        matcher.Matches("thing/notanumber", routeValues).ShouldBeFalse();

        routeValues.ShouldBeEmpty();
    }

    [Fact]
    public void Matches_NullableMatcherYieldingNull_StoresNull()
    {
        var builder = new FluentRouterConfigurationOptionsBuilder();
        builder.AddSegmentMatcher("optint", new OptionalIntSegmentMatcher());
        var matcher = new RouteMatcherCompiler(builder.BuildConfiguration()).Compile("counter/{Id:optint}");
        var routeValues = new Dictionary<string, object>();

        Should.NotThrow(() => matcher.Validate(typeof(NullableIntParameterPage)));

        matcher.Matches("counter/none", routeValues).ShouldBeTrue();
        routeValues["Id"].ShouldBeNull();

        routeValues.Clear();
        matcher.Matches("counter/5", routeValues).ShouldBeTrue();
        routeValues["Id"].ShouldBe(5);
    }

    private sealed class OptionalIntSegmentMatcher : SegmentMatcherBase<int?>
    {
        public override bool MatchSegment(string segment, out object segmentValue)
        {
            segmentValue = null!;
            if (segment == "none")
            {
                return true;
            }

            if (!int.TryParse(segment, out var value))
            {
                return false;
            }

            segmentValue = value;
            return true;
        }
    }

    [Fact]
    public void Matches_ExistingRouteValues_AreOverwrittenOnSuccess()
    {
        var routeValues = new Dictionary<string, object> { ["Id"] = 1 };

        Compile("counter/{Id:int}").Matches("counter/42", routeValues).ShouldBeTrue();

        routeValues["Id"].ShouldBe(42);
    }
}
