
# FluentBlazorRouter

FluentBlazorRouter is an alternative router for blazor applications.
It allows you to use dynamic groups and nesting instead of using hardcoded compile time constatnts to route to your amazing blazor pages.
And all that without repeating yourself! (The future is now!)
## Features

- central route configuration, no more hopping around in multiple files
- nested routes and groups/hierarchical routing
- customizable route parameter parsing
- route parameter type validation at application startup

Currently query parameters are not supported.

### builtin route parameter types

Here are the builtin route parameter types:

- string
- long
- int
- short
- byte
- guid

You can extend these yourself in the *IServiceCollection* extension method.
(See below)
## Installation

You can simply install *FluentBlazorRouter* from nuget.
Either via the command line or with your IDE of choice.

```
  dotnet add package FluentBlazorRouter
```

## Usage/Examples

First of: you can remove all the `@page` directives in all blazor pages.
You won't need them anymore.
Also not on any new pages.

👋 Goodbye 👋

Now lets see how its done with *FluentBlazorRouter*.

Either add the following line into your *_Imports.razor* or directly in your *App.razor* file.

```c#
@using FluentBlazorRouter
```

Then in your *App.razor* replace the default Router with *<FluentRouter>*.
It can be used as a drop in replacement.

It should look something like this:

```c#
@using FluentBlazorRouter 

<FluentRouter>
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)"/>
        <FocusOnNavigate RouteData="@routeData" Selector="h1"/>
    </Found>
    <NotFound>
        <PageTitle>Not found</PageTitle>
        <LayoutView Layout="@typeof(MainLayout)">
            <p role="alert">Sorry, there's nothing at this address.</p>
        </LayoutView>
    </NotFound>
</FluentRouter>
```

Finally in your *Program.cs* file (in earlier versions this was in the *Startup.cs* file) you configure the router:

```c#
builder.Services.AddFluentRouting<FluentBlazorRouter.Test.Pages.Index>(rootBuilder => rootBuilder
    .WithPage<Counter>("counter/{Id:int}")
    .WithGroup("group/example", exampleGroupBuilder =>
    {
        exampleGroupBuilder.WithPage<FetchData>("fetchdata");
    }));
```

Note that pages can also have sub pages.

### Getting the runtime route of a page

To get the route of a page at runtime simply inject a *IRouteProvider* instance and use one of the *TryGetPageRoute* methods.

Example from the Test project *Counter.razor* page:

```c#
@if (RouteProvider.TryGetPageRoute<Counter>(out var route))
{
    <p>relative url: @route</p>
}
else
{
    <p>how did you get here?</p>
}
```

### Custom route parameter matchers

You can easily add your own custom route parameter matcher and parsers:

```c#
builder.Services.AddFluentRouting<FluentBlazorRouter.Test.Pages.Index>(...),// reduced for brevity
    optionsBuilder =>
    {
        optionsBuilder.AddSegmentMatcher("custom", new CustomMatcher());
    });
```

### Router Middlewares

The router is capable of executing middlewares for additional routing decision making and logging.
This can be used to implement custom validation and/or permission logic.

To add a middleware simply implement the *IRouterMiddlware* interface and make sure to call *next()* to execute the next middleware.
Not calling *next()* will result in rendering the *PageNotFound* template. 

```c#
public class MyMiddleware : IRouterMiddleware
{
    public void Execute(Action next, RouteData pageContext)
    {
        if (pageContext.PageType != typeof(FetchData))
        {
            next();
        }
    }
}
```

And this in your *Program.cs*:

```c#
builder.Services.AddTransient<IRouterMiddleware, MyMiddleware>();
```

## Feedback

If you have any feedback, please reach out to me on Discord Darkarotte#8994.


## License

[MIT](https://choosealicense.com/licenses/mit/)

