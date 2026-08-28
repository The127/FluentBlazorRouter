using Microsoft.AspNetCore.Components;

namespace FluentBlazorRouter.UnitTests;

internal sealed class IntParameterPage
{
    [Parameter] public int Id { get; set; }
}

internal sealed class StringParameterPage
{
    [Parameter] public string Name { get; set; } = string.Empty;
}

internal sealed class NoParameterAttributePage
{
    public int Id { get; set; }
}

internal sealed class WrongTypePage
{
    [Parameter] public string Id { get; set; } = string.Empty;
}

internal sealed class NoPropertyPage
{
}

internal sealed class NullableIntParameterPage
{
    [Parameter] public int? Id { get; set; }
}

internal sealed class NullableIntNoAttributePage
{
    public int? Id { get; set; }
}

internal sealed class NullableLongParameterPage
{
    [Parameter] public long? Id { get; set; }
}

internal sealed class NonPublicPropertyPage
{
    internal int Id { get; set; }
}

internal sealed class StaticParameterPage
{
    [Parameter] public static int Id { get; set; }
}

internal class PlainBasePage
{
    public int Id { get; set; }
}

internal sealed class ShadowedWrongTypePage : PlainBasePage
{
    [Parameter] public new string Id { get; set; } = string.Empty;
}

internal sealed class ShadowedCompatibleTypePage : PlainBasePage
{
    [Parameter] public new int? Id { get; set; }
}

internal class VirtualNoAttributeBasePage
{
    public virtual int Id { get; set; }
}

internal sealed class OverriddenOnlyParameterPage : VirtualNoAttributeBasePage
{
    [Parameter] public override int Id { get; set; }
}

internal class AttributeOnBasePage
{
    [Parameter] public int Id { get; set; }
}

internal sealed class ShadowedWithoutAttributePage : AttributeOnBasePage
{
    public new string Id { get; set; } = string.Empty;
}

internal class VirtualParameterBasePage
{
    [Parameter] public virtual int Id { get; set; }
}

internal sealed class OverriddenParameterPage : VirtualParameterBasePage
{
    [Parameter] public override int Id { get; set; }
}

internal sealed class IndexerPage
{
    public int this[int index] => index;

    public int this[string key] => key.Length;
}

internal class ProtectedBasePage
{
    protected int Id { get; set; }
}

internal sealed class ShadowedNonPublicPage : ProtectedBasePage
{
    private new string Id { get; set; } = string.Empty;
}

internal sealed class DigitNamedParameterPage
{
    [Parameter] public int Page2 { get; set; }
}

internal sealed class UnderscoreNamedParameterPage
{
    [Parameter] public int My_Id { get; set; }
}

internal sealed class UnicodeNamedParameterPage
{
    [Parameter] public int Größe { get; set; }
}

internal class BasePage
{
    [Parameter] public int Id { get; set; }
}

internal sealed class InheritedParameterPage : BasePage
{
}

internal class NullableBasePage
{
    [Parameter] public int? Id { get; set; }
}

internal sealed class InheritedNullableParameterPage : NullableBasePage
{
}

internal sealed class ShadowedParameterPage : BasePage
{
    [Parameter] public new int? Id { get; set; }
}
