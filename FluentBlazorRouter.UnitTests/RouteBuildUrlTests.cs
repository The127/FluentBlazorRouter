using System.Globalization;
using FluentBlazorRouter.Internal;
using Shouldly;
using Xunit;

namespace FluentBlazorRouter.UnitTests;

public class RouteBuildUrlTests
{
    private static Route CreateRoute(string fullRoute, FluentRouterConfigurationOptionsBuilder? builder = null)
        => new(fullRoute, typeof(IntParameterPage),
            new RouteMatcherCompiler((builder ?? new FluentRouterConfigurationOptionsBuilder()).BuildConfiguration()),
            new Dictionary<Type, object>(), null);

    [Fact]
    public void BuildUrl_SubstitutesTypedParameter()
        => CreateRoute("counter/{Id:int}")
            .BuildUrl(new Dictionary<string, object?> { ["Id"] = 42 })
            .ShouldBe("counter/42");

    [Fact]
    public void BuildUrl_KeepsStaticSegments()
        => CreateRoute("group/example/{Id:int}")
            .BuildUrl(new Dictionary<string, object?> { ["Id"] = 1 })
            .ShouldBe("group/example/1");

    [Fact]
    public void BuildUrl_StaticRoute_NeedsNoValues()
        => CreateRoute("group/example")
            .BuildUrl(new Dictionary<string, object?>())
            .ShouldBe("group/example");

    [Fact]
    public void BuildUrl_MissingValue_Throws()
        => Should.Throw<InvalidOperationException>(
                () => CreateRoute("counter/{Id:int}").BuildUrl(new Dictionary<string, object?>()))
            .Message.ShouldBe("No route value provided for 'Id'.");

    [Fact]
    public void BuildUrl_NullValueWithDefaultFormatter_Throws()
    {
        var builder = new FluentRouterConfigurationOptionsBuilder();
        builder.AddSegmentMatcher("nint", new NullableIntMatcher());

        Should.Throw<InvalidOperationException>(
                () => CreateRoute("counter/{Id:nint}", builder).BuildUrl(new Dictionary<string, object?> { ["Id"] = null }))
            .Message.ShouldContain("Cannot format a null route value");
    }

    [Fact]
    public void BuildUrl_MatcherWithFormatValue_RoundTripsNull()
    {
        var builder = new FluentRouterConfigurationOptionsBuilder();
        builder.AddSegmentMatcher("optint", new OptionalIntMatcher());
        var route = CreateRoute("counter/{Id:optint}", builder);

        var routeValues = new Dictionary<string, object?>();
        route.Matches("counter/none", routeValues).ShouldBeTrue();
        routeValues["Id"].ShouldBeNull();

        route.BuildUrl(routeValues).ShouldBe("counter/none");
    }

    [Fact]
    public void BuildUrl_MatcherWithFormatValue_RoundTripsValue()
    {
        var builder = new FluentRouterConfigurationOptionsBuilder();
        builder.AddSegmentMatcher("optint", new OptionalIntMatcher());
        var route = CreateRoute("counter/{Id:optint}", builder);

        var routeValues = new Dictionary<string, object?>();
        route.Matches("counter/5", routeValues).ShouldBeTrue();

        route.BuildUrl(routeValues).ShouldBe("counter/5");
    }

    [Fact]
    public void BuildUrl_NullRouteValues_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(() => CreateRoute("counter/{Id:int}").BuildUrl(null!))
            .ParamName.ShouldBe("routeValues");

    [Fact]
    public void BuildUrl_ValueOfWrongType_Throws()
        => Should.Throw<InvalidOperationException>(
                () => CreateRoute("counter/{Id:int}").BuildUrl(new Dictionary<string, object?> { ["Id"] = "abc" }))
            .Message.ShouldContain("does not match the expected type");

    [Fact]
    public void BuildUrl_ValueContainingPathSeparator_Throws()
        => Should.Throw<InvalidOperationException>(
                () => CreateRoute("user/{Name}").BuildUrl(new Dictionary<string, object?> { ["Name"] = "a/b" }))
            .Message.ShouldContain("contains a path separator");

    [Fact]
    public void BuildUrl_NullableValue_UsesUnderlyingType()
        => CreateRoute("counter/{Id:int}")
            .BuildUrl(new Dictionary<string, object?> { ["Id"] = (int?)7 })
            .ShouldBe("counter/7");

    [Theory]
    [InlineData("de-DE")]
    [InlineData("en-US")]
    public void BuildUrl_RoundTripsUnderAnyCulture(string culture)
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            var route = CreateRoute("price/{Amount:double}");

            var url = route.BuildUrl(new Dictionary<string, object?> { ["Amount"] = 1.5d });
            url.ShouldBe("price/1.5");

            var routeValues = new Dictionary<string, object?>();
            route.Matches(url, routeValues).ShouldBeTrue();
            routeValues["Amount"].ShouldBe(1.5d);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    private class NullableIntMatcher : SegmentMatcherBase<int?>
    {
        public override bool MatchSegment(string segment, out object? segmentValue)
        {
            segmentValue = int.TryParse(segment, out var value) ? value : null;
            return true;
        }
    }

    private sealed class OptionalIntMatcher : NullableIntMatcher
    {
        public override string FormatValue(object? value) => value?.ToString() ?? "none";

        public override bool MatchSegment(string segment, out object? segmentValue)
        {
            segmentValue = null;
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
}
