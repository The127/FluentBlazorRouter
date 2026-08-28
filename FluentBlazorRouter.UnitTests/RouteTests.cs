using FluentBlazorRouter.Internal;
using Shouldly;
using Xunit;

namespace FluentBlazorRouter.UnitTests;

public class RouteTests
{
    private static RouteMatcherCompiler Compiler() =>
        new(new FluentRouterConfigurationOptionsBuilder().BuildConfiguration());

    private static Route CreateRoute(
        string fullRoute = "counter/{Id:int}",
        Type? pageType = null,
        IReadOnlyDictionary<Type, object>? metadata = null,
        Route? parent = null)
        => new(fullRoute, pageType ?? typeof(IntParameterPage), Compiler(),
            metadata ?? new Dictionary<Type, object>(), parent);

    [Fact]
    public void Route_ExposesTheValuesItWasBuiltWith()
    {
        var route = CreateRoute();

        route.FullRoute.ShouldBe("counter/{Id:int}");
        route.PageType.ShouldBe(typeof(IntParameterPage));
        route.Parent.ShouldBeNull();
    }

    [Fact]
    public void Route_KeepsItsParent()
    {
        var parent = CreateRoute("group/example", typeof(NoPropertyPage));

        CreateRoute(parent: parent).Parent.ShouldBe(parent);
    }

    [Fact]
    public void Matches_DelegatesToTheCompiledMatcher()
    {
        var routeValues = new Dictionary<string, object>();

        CreateRoute().Matches("counter/42", routeValues).ShouldBeTrue();

        routeValues["Id"].ShouldBe(42);
    }

    [Fact]
    public void Validate_DelegatesToTheCompiledMatcher()
    {
        Should.NotThrow(() => CreateRoute().Validate());
        Should.Throw<Exception>(() => CreateRoute(pageType: typeof(NoPropertyPage)).Validate());
    }

    [Fact]
    public void TryGetMetadata_ReturnsRegisteredMetadata()
    {
        var route = CreateRoute(metadata: new Dictionary<Type, object> { [typeof(string)] = "Counter" });

        route.TryGetMetadata<string>(out var metadata).ShouldBeTrue();
        metadata.ShouldBe("Counter");
    }

    [Fact]
    public void TryGetMetadata_ReturnsFalseWhenAbsent()
    {
        CreateRoute().TryGetMetadata<string>(out var metadata).ShouldBeFalse();
        metadata.ShouldBeNull();
    }

    [Fact]
    public void TryGetMetadata_ByType_ReturnsRegisteredMetadata()
    {
        var route = CreateRoute(metadata: new Dictionary<Type, object> { [typeof(string)] = "Counter" });

        route.TryGetMetadata(typeof(string), out var metadata).ShouldBeTrue();
        metadata.ShouldBe("Counter");
    }
}
