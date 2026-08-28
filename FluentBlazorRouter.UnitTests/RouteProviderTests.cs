using FluentBlazorRouter.Internal;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace FluentBlazorRouter.UnitTests;

public class RouteProviderTests
{
    private sealed class PageA : ComponentBase
    {
        [Parameter] public int Id { get; set; }
    }

    private sealed class PageB : ComponentBase
    {
        [Parameter] public int Id { get; set; }
    }

    private static RouteProvider CreateProvider(Action<RouteGroupBuilder> configure)
    {
        var compiler = new RouteMatcherCompiler(new FluentRouterConfigurationOptionsBuilder().BuildConfiguration());
        var root = new RouteGroupBuilder(string.Empty, typeof(PageA), compiler);
        configure(root);
        return new RouteProvider(root);
    }

    [Theory]
    [InlineData("first/7", "first/{Id:int}")]
    [InlineData("second/7", "second/{Id:int}")]
    public void TryMatch_PageRegisteredTwice_ReturnsTheRouteThatMatched(string url, string expectedRoute)
    {
        var provider = CreateProvider(root => root
            .WithPage<PageB>("first/{Id:int}")
            .WithPage<PageB>("second/{Id:int}"));

        provider.TryMatch(url, out var routeValues, out Route? route).ShouldBeTrue();

        route.FullRoute.ShouldBe(expectedRoute);
        route.PageType.ShouldBe(typeof(PageB));
        routeValues["Id"].ShouldBe(7);
    }

    [Fact]
    public void TryMatch_BuildUrlOnTheMatchedRoute_RoundTrips()
    {
        var provider = CreateProvider(root => root
            .WithPage<PageB>("first/{Id:int}")
            .WithPage<PageB>("second/{Id:int}"));

        provider.TryMatch("second/7", out var routeValues, out Route? route).ShouldBeTrue();

        route.BuildUrl(routeValues).ShouldBe("second/7");
    }

    [Fact]
    public void TryMatch_NoRouteMatches_ReturnsFalse()
    {
        var provider = CreateProvider(root => root.WithPage<PageB>("first/{Id:int}"));

        provider.TryMatch("nope/7", out _, out Route? route).ShouldBeFalse();
        route.ShouldBeNull();
    }

    [Fact]
    public void TryGetRouteData_PageRegisteredTwice_ReturnsTheFirstRegistration_KnownLimitation()
    {
        var provider = CreateProvider(root => root
            .WithPage<PageB>("first/{Id:int}")
            .WithPage<PageB>("second/{Id:int}"));

        provider.TryGetRouteData(typeof(PageB), out var route).ShouldBeTrue();

        route.FullRoute.ShouldBe("first/{Id:int}");
    }
}
