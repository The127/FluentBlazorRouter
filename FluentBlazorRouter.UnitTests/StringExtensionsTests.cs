using Shouldly;
using Xunit;

namespace FluentBlazorRouter.UnitTests;

#pragma warning disable CS0618

public class StringExtensionsTests
{
    [Fact]
    public void ApplyRouteValues_Dictionary_SubstitutesUntypedPlaceholder()
        => "user/{id}".ApplyRouteValues(new Dictionary<string, object?> { ["id"] = 1 })
            .ShouldBe("user/1");

    [Fact]
    public void ApplyRouteValues_Dictionary_SubstitutesTypedPlaceholder()
        => "user/{id:int}".ApplyRouteValues(new Dictionary<string, object?> { ["id"] = 1 })
            .ShouldBe("user/1");

    [Fact]
    public void ApplyRouteValues_Dictionary_SubstitutesEveryPlaceholder()
        => "{area}/user/{id:int}".ApplyRouteValues(new Dictionary<string, object?>
            {
                ["area"] = "admin",
                ["id"] = 7,
            })
            .ShouldBe("admin/user/7");

    [Fact]
    public void ApplyRouteValues_Dictionary_LeavesUnknownPlaceholderInPlace()
        => "user/{id:int}".ApplyRouteValues(new Dictionary<string, object?>())
            .ShouldBe("user/{id:int}");

    [Fact]
    public void ApplyRouteValues_Dictionary_IgnoresWhitespaceInsidePlaceholder()
        => "user/{ id : int }".ApplyRouteValues(new Dictionary<string, object?> { ["id"] = 1 })
            .ShouldBe("user/1");

    [Fact]
    public void ApplyRouteValues_Dictionary_LeavesRouteWithoutPlaceholdersAlone()
        => "group/example".ApplyRouteValues(new Dictionary<string, object?>())
            .ShouldBe("group/example");

    [Theory]
    [InlineData("page/{Page2:int}", "Page2")]
    [InlineData("page/{My_Id:int}", "My_Id")]
    [InlineData("page/{_leading}", "_leading")]
    [InlineData("page/{Größe:int}", "Größe")]
    public void ApplyRouteValues_Dictionary_SubstitutesNamesWithDigitsOrUnderscores(string route, string key)
        => route.ApplyRouteValues(new Dictionary<string, object?> { [key] = 7 }).ShouldBe("page/7");

    [Fact]
    public void ApplyRouteValues_Dictionary_IgnoresNamesTheCompilerWouldReject()
        => "user/{1id}".ApplyRouteValues(new Dictionary<string, object?> { ["1id"] = 3 })
            .ShouldBe("user/{1id}");

    [Fact]
    public void ApplyRouteValues_Dictionary_NullValue_ThrowsNamingTheKey()
        => Should.Throw<InvalidOperationException>(
                () => "user/{id:int}".ApplyRouteValues(new Dictionary<string, object?> { ["id"] = null! }))
            .Message.ShouldBe("Route value 'id' is null.");

    [Fact]
    public void ApplyRouteValues_Dictionary_ValueWithNullToString_ThrowsNamingTheKey()
        => Should.Throw<InvalidOperationException>(
                () => "user/{id}".ApplyRouteValues(new Dictionary<string, object?> { ["id"] = new NullToString() }))
            .Message.ShouldBe("Route value 'id' has a null string representation.");

    [Fact]
    public void ApplyRouteValues_Dictionary_NullDictionary_ThrowsArgumentNullException()
    {
        Dictionary<string, object?> routeValues = null!;

        Should.Throw<ArgumentNullException>(() => "user/{id}".ApplyRouteValues(routeValues))
            .ParamName.ShouldBe("routeValues");
    }

    [Fact]
    public void ApplyRouteValues_Positional_NullArray_ThrowsArgumentNullException()
    {
        object[] routeValues = null!;

        Should.Throw<ArgumentNullException>(() => "user/{0}".ApplyRouteValues(routeValues))
            .ParamName.ShouldBe("routeValues");
    }

    [Fact]
    public void ApplyRouteValues_Positional_NullValue_ThrowsNamingTheIndex()
        => Should.Throw<InvalidOperationException>(() => "user/{0}".ApplyRouteValues(new object?[] { null }))
            .Message.ShouldBe("Route value at index 0 is null.");

    [Fact]
    public void ApplyRouteValues_Positional_SubstitutesByIndex()
        => "user/{0}/post/{1}".ApplyRouteValues("alice", 3).ShouldBe("user/alice/post/3");

    [Fact]
    public void ApplyRouteValues_Positional_LeavesOutOfRangeIndexInPlace()
        => "user/{5}".ApplyRouteValues("alice").ShouldBe("user/{5}");

    [Fact]
    public void ApplyRouteValues_Positional_NamedPlaceholderThrows()
        => Should.Throw<FormatException>(() => "user/{id}".ApplyRouteValues("alice"));

    private sealed class NullToString
    {
        public override string? ToString() => null;
    }
}

#pragma warning restore CS0618
