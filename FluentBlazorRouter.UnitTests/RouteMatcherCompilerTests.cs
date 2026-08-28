using System.Text;
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

        matcher.Matches("group/example", new Dictionary<string, object?>()).ShouldBeTrue();
        matcher.Matches("group/other", new Dictionary<string, object?>()).ShouldBeFalse();
    }

    [Fact]
    public void Compile_UntypedParameter_DefaultsToStringMatcher()
    {
        var matcher = CreateCompiler().Compile("page/{ActiveTab}");
        var routeValues = new Dictionary<string, object?>();

        matcher.Matches("page/anything", routeValues).ShouldBeTrue();
        routeValues.ShouldContainKeyAndValue("ActiveTab", "anything");
    }

    [Fact]
    public void Compile_TypedParameter_UsesTheRegisteredMatcher()
    {
        var matcher = CreateCompiler().Compile("counter/{Id:int}");
        var routeValues = new Dictionary<string, object?>();

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
    [InlineData("{_leading}")]
    [InlineData("{Id1:int}")]
    public void Compile_ParameterNameWithDigitOrUnderscore_IsAccepted(string segment)
        => Should.NotThrow(() => CreateCompiler().Compile(segment));

    [Theory]
    [InlineData("{1Id}")]
    [InlineData("{Id:1int}")]
    public void Compile_NameStartingWithDigit_Throws(string segment)
        => Should.Throw<Exception>(() => CreateCompiler().Compile(segment))
            .Message.ShouldContain("Route segment error");

    [Fact]
    public void Compile_RouteForPropertyNameContainingDigit_Validates()
    {
        var matcher = CreateCompiler().Compile("page/{Page2:int}");

        Should.NotThrow(() => matcher.Validate(typeof(DigitNamedParameterPage)));
        matcher.Matches("page/7", new Dictionary<string, object?>()).ShouldBeTrue();
    }

    [Fact]
    public void Compile_RouteForPropertyNameContainingUnderscore_Validates()
    {
        var matcher = CreateCompiler().Compile("page/{My_Id:int}");

        Should.NotThrow(() => matcher.Validate(typeof(UnderscoreNamedParameterPage)));
        matcher.Matches("page/7", new Dictionary<string, object?>()).ShouldBeTrue();
    }

    [Fact]
    public void Compile_RouteForNonAsciiPropertyName_Validates()
    {
        var matcher = CreateCompiler().Compile("page/{Größe:int}");

        Should.NotThrow(() => matcher.Validate(typeof(UnicodeNamedParameterPage)));
        matcher.Matches("page/7", new Dictionary<string, object?>()).ShouldBeTrue();
    }

    [Fact]
    public void Compile_DecomposedNonAsciiPropertyName_IsAccepted()
        => Should.NotThrow(
            () => CreateCompiler().Compile("page/{" + "Größe".Normalize(NormalizationForm.FormD) + ":int}"));

    [Fact]
    public void AddSegmentMatcher_NullKey_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(
                () => new FluentRouterConfigurationOptionsBuilder().AddSegmentMatcher(null!, new EvenNumberMatcher()))
            .ParamName.ShouldBe("segmentIdentifier");

    [Theory]
    [InlineData("date-time")]
    [InlineData("trailing\n")]
    [InlineData("with space")]
    [InlineData("1leading")]
    [InlineData("")]
    public void AddSegmentMatcher_KeyThatCannotAppearInARoute_Throws(string key)
        => Should.Throw<ArgumentException>(
                () => new FluentRouterConfigurationOptionsBuilder().AddSegmentMatcher(key, new EvenNumberMatcher()))
            .Message.ShouldContain("cannot be used in a route");

    [Theory]
    [InlineData("int32")]
    [InlineData("date_time")]
    [InlineData("_custom")]
    public void AddSegmentMatcher_KeyIsUsableInARoute(string key)
    {
        var builder = new FluentRouterConfigurationOptionsBuilder();
        builder.AddSegmentMatcher(key, new EvenNumberMatcher());
        var compiler = new RouteMatcherCompiler(builder.BuildConfiguration());

        var matcher = Should.NotThrow(() => compiler.Compile("n/{Value:" + key + "}"));

        var routeValues = new Dictionary<string, object?>();
        matcher.Matches("n/4", routeValues).ShouldBeTrue();
        routeValues["Value"].ShouldBe(4);
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

        var routeValues = new Dictionary<string, object?>();
        matcher.Matches("n/4", routeValues).ShouldBeTrue();
        routeValues["Value"].ShouldBe(4);

        matcher.Matches("n/5", new Dictionary<string, object?>()).ShouldBeFalse();
    }

    private sealed class EvenNumberMatcher : SegmentMatcherBase<int>
    {
        public override bool MatchSegment(string segment, out object? segmentValue)
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
