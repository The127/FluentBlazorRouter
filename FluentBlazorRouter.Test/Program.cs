using FluentBlazorRouter;
using FluentBlazorRouter.Test.Data;
using FluentBlazorRouter.Test.Pages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddFluentRouting<FluentBlazorRouter.Test.Pages.Index>(rootBuilder => rootBuilder
    .WithPage<Counter>("counter/{Id:int}")
    .WithGroup("group/example", exampleGroupBuilder =>
    {
        exampleGroupBuilder.WithPage<FetchData>("fetchdata");
    }));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();