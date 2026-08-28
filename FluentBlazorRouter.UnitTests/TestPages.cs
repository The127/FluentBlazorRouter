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

internal class BasePage
{
    [Parameter] public int Id { get; set; }
}

internal sealed class InheritedParameterPage : BasePage
{
}

internal sealed class ShadowedParameterPage : BasePage
{
    [Parameter] public new int? Id { get; set; }
}
