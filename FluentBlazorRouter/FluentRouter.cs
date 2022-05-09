using System.Collections;
using System.ComponentModel;
using System.Web;
using FluentBlazorRouter.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using IComponent = Microsoft.AspNetCore.Components.IComponent;

namespace FluentBlazorRouter;

public sealed class FluentRouter : IComponent, IHandleAfterRender, IDisposable
{
    private RenderHandle _renderHandle;
    private string _location = null!;
    private bool _isFirstRender = true;
    
    
    [Parameter]
    public RenderFragment<RouteData> Found { get; set; } = null!;
    
    [Parameter]
    public RenderFragment NotFound { get; set; } = null!;

    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    [Inject] private IRouteProvider RouteProvider { get; set; } = null!;
    [Inject] private INavigationInterception NavigationInterception { get; set; } = null!;

    [Inject] private IServiceProvider ServiceProvider { get; set; } = null!;


    void IComponent.Attach(RenderHandle renderHandle)
    {
        _renderHandle = renderHandle;
        _location = NavigationManager.Uri;
        
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _location = e.Location;
        Refresh();
    }

    private void Refresh()
    {
        var relativeUri = NavigationManager.ToBaseRelativePath(_location);

        var questionMarkIndex = relativeUri.IndexOf('?');
        if (questionMarkIndex > -1)
        {
            relativeUri = relativeUri[..questionMarkIndex];
        }

        if (RouteProvider.TryMatch(relativeUri, out var routeValues,out var pageType))
        {
            var middlewares = (IEnumerable<IRouterMiddleware>)(ServiceProvider.GetService(typeof(IEnumerable<IRouterMiddleware>)) ?? Enumerable.Empty<IRouterMiddleware>());

            var routeData = new RouteData(
                pageType,
                routeValues
            );
            
            var renderPage = true;
            foreach (var routerMiddleware in middlewares)
            {
                var executeNext = false;

                void NextCallback()
                {
                    executeNext = true;
                }

                routerMiddleware.Execute(NextCallback, routeData);

                if (!executeNext)
                {
                    renderPage = false;
                    break;
                }
            }

            if (renderPage)
            {
                _renderHandle.Render(Found(routeData));
                return;
            }
        }
        
        _renderHandle.Render(NotFound);
    }

    Task IComponent.SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);

        if (Found is null)
        {
            throw new InvalidOperationException($"The {nameof(FluentRouter)} component requires a value for the parameter {nameof(Found)}.");
        }

        if (NotFound is null)
        {
            throw new InvalidOperationException($"The {nameof(FluentRouter)} component requires a value for the parameter {nameof(NotFound)}.");
        }

        Refresh();
        
        return Task.CompletedTask;
    }
    
    public Task OnAfterRenderAsync()
    {
        if (!_isFirstRender) return Task.CompletedTask;
        
        _isFirstRender = false;
        return NavigationInterception.EnableNavigationInterceptionAsync();
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}