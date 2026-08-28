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
    public void Validate_NullableParameterProperty_ThrowsToday_Issue5()
        => Should.Throw<Exception>(() => Compile("counter/{Id:int}").Validate(typeof(NullableIntParameterPage)))
            .Message.ShouldContain("No property 'Id'");

    [Fact]
    public void Validate_PropertyShadowedWithDifferentType_ThrowsAmbiguousMatch()
        => Should.Throw<AmbiguousMatchException>(
            () => Compile("counter/{Id:int}").Validate(typeof(ShadowedParameterPage)));
}
