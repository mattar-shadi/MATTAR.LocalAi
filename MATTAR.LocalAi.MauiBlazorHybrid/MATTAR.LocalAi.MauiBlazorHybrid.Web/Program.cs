using MATTAR.LocalAi.MauiBlazorHybrid.Shared.Services;
using MATTAR.LocalAi.MauiBlazorHybrid.Web.Components;
using MATTAR.LocalAi.MauiBlazorHybrid.Web.Services;
using Microsoft.AspNetCore.Components.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<CircuitOptions>(options =>
{
    options.DetailedErrors = true;
});

// Add device-specific services used by the MATTAR.LocalAi.MauiBlazorHybrid.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(MATTAR.LocalAi.MauiBlazorHybrid.Shared._Imports).Assembly);

app.Run();
