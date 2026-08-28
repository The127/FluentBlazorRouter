using Shouldly;
using Xunit;

namespace FluentBlazorRouter.UnitTests;

public class FluentRouterTests
{
    [Theory]
    [InlineData("counter/5", "counter/5")]
    [InlineData("counter/5?x=1", "counter/5")]
    [InlineData("counter/5#details", "counter/5")]
    [InlineData("counter/5?x=1#details", "counter/5")]
    [InlineData("counter/5#details?x=1", "counter/5")]
    [InlineData("", "")]
    public void StripQueryAndFragment_RemovesBoth(string relativeUri, string expected)
        => FluentRouter.StripQueryAndFragment(relativeUri).ShouldBe(expected);
}
