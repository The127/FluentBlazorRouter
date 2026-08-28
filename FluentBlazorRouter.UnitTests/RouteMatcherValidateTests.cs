using System.Reflection;
using FluentBlazorRouter.Internal;
using Shouldly;
using Xunit;

namespace FluentBlazorRouter.UnitTests;

public class RouteMatcherValidateTests
{
    private static RouteMatcher Compile(string route) =>
        new RouteMatcherCompiler(new FluentRouterConfigurationOptionsBuilder().BuildConfiguration()).Compile(route);

    [Fact]
    public void Validate_MatchingTypedParameter_DoesNotThrow()
        => Should.NotThrow(() => Compile("counter/{Id:int}").Validate(typeof(IntParameterPage)));

    [Fact]
    public void Validate_MatchingUntypedParameter_DoesNotThrow()
        => Should.NotThrow(() => Compile("page/{Name}").Validate(typeof(StringParameterPage)));

    [Fact]
    public void Validate_StaticRoute_NeedsNoProperties()
        => Should.NotThrow(() => Compile("group/example").Validate(typeof(NoPropertyPage)));

    [Fact]
    public void Validate_PropertyMissing_Throws()
        => Should.Throw<Exception>(() => Compile("counter/{Id:int}").Validate(typeof(NoPropertyPage)))
            .Message.ShouldContain("No property 'Id'");

    [Fact]
    public void Validate_PropertyHasWrongType_Throws()
        => Should.Throw<Exception>(() => Compile("counter/{Id:int}").Validate(typeof(WrongTypePage)))
            .Message.ShouldContain("No property 'Id'");

    [Fact]
    public void Validate_PropertyLacksParameterAttribute_Throws()
        => Should.Throw<Exception>(() => Compile("counter/{Id:int}").Validate(typeof(NoParameterAttributePage)))
            .Message.ShouldContain("No property 'Id'");

    [Fact]
    public void Validate_InheritedParameterProperty_DoesNotThrow()
        => Should.NotThrow(() => Compile("counter/{Id:int}").Validate(typeof(InheritedParameterPage)));

    [Fact]
    public void Validate_InheritedNullableParameterProperty_DoesNotThrow()
        => Should.NotThrow(
            () => Compile("counter/{Id:int}").Validate(typeof(InheritedNullableParameterPage)));

    [Fact]
    public void Validate_NullableParameterProperty_DoesNotThrow()
        => Should.NotThrow(() => Compile("counter/{Id:int}").Validate(typeof(NullableIntParameterPage)));

    [Fact]
    public void Validate_NullableParameterProperty_StillRequiresParameterAttribute()
        => Should.Throw<Exception>(
                () => Compile("counter/{Id:int}").Validate(typeof(NullableIntNoAttributePage)))
            .Message.ShouldContain("No property 'Id'");

    [Fact]
    public void Validate_NullableParameterProperty_StillRequiresMatchingUnderlyingType()
        => Should.Throw<Exception>(
                () => Compile("counter/{Id:int}").Validate(typeof(NullableLongParameterPage)))
            .Message.ShouldContain("No property 'Id'");

    [Fact]
    public void Validate_NullableMatchTypeWithNullableProperty_DoesNotThrow()
    {
        var builder = new FluentRouterConfigurationOptionsBuilder();
        builder.AddSegmentMatcher("nint", new NullableIntSegmentMatcher());
        var matcher = new RouteMatcherCompiler(builder.BuildConfiguration()).Compile("counter/{Id:nint}");

        Should.NotThrow(() => matcher.Validate(typeof(NullableIntParameterPage)));
    }

    [Fact]
    public void Validate_NullableMatchTypeWithNonNullableProperty_Throws()
    {
        var builder = new FluentRouterConfigurationOptionsBuilder();
        builder.AddSegmentMatcher("nint", new NullableIntSegmentMatcher());
        var matcher = new RouteMatcherCompiler(builder.BuildConfiguration()).Compile("counter/{Id:nint}");

        Should.Throw<Exception>(() => matcher.Validate(typeof(IntParameterPage)))
            .Message.ShouldContain("No property 'Id'");
    }

    [Fact]
    public void Validate_NullableMatchTypeWithWrongProperty_Throws()
    {
        var builder = new FluentRouterConfigurationOptionsBuilder();
        builder.AddSegmentMatcher("nint", new NullableIntSegmentMatcher());
        var matcher = new RouteMatcherCompiler(builder.BuildConfiguration()).Compile("counter/{Id:nint}");

        Should.Throw<Exception>(() => matcher.Validate(typeof(WrongTypePage)))
            .Message.ShouldContain("No property 'Id'");
    }

    [Fact]
    public void Validate_MatcherWithNullMatchType_Throws()
    {
        var builder = new FluentRouterConfigurationOptionsBuilder();
        builder.AddSegmentMatcher("bad", new NullMatchTypeSegmentMatcher());
        var matcher = new RouteMatcherCompiler(builder.BuildConfiguration()).Compile("counter/{Id:bad}");

        Should.Throw<Exception>(() => matcher.Validate(typeof(IntParameterPage)))
            .Message.ShouldContain("No property 'Id'");
    }

    private sealed class NullMatchTypeSegmentMatcher : ISegmentMatcher
    {
        public Type MatchType => null!;

        public bool MatchSegment(string segment, out object segmentValue)
        {
            segmentValue = 0;
            return true;
        }
    }

    private sealed class NullableIntSegmentMatcher : SegmentMatcherBase<int?>
    {
        public override bool MatchSegment(string segment, out object segmentValue)
        {
            segmentValue = 0;
            if (!int.TryParse(segment, out var value))
            {
                return false;
            }

            segmentValue = value;
            return true;
        }
    }

    [Fact]
    public void Validate_PropertyShadowedWithDifferentType_ThrowsAmbiguousMatch()
        => Should.Throw<AmbiguousMatchException>(
            () => Compile("counter/{Id:int}").Validate(typeof(ShadowedParameterPage)));
}
