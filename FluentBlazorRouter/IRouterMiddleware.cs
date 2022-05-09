using Microsoft.AspNetCore.Components;

namespace FluentBlazorRouter;

public interface IRouterMiddleware
{
    void Execute(Action next, RouteData pageContext);
}

