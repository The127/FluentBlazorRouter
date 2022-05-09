using FluentBlazorRouter.Test.Pages;
using RouteData = Microsoft.AspNetCore.Components.RouteData;

namespace FluentBlazorRouter.Test;

public class TestMiddleware : IRouterMiddleware
{
    public void Execute(Action next, RouteData pageContext)
    {
        if (pageContext.PageType != typeof(FetchData))
        {
            next();
        }
    }
}