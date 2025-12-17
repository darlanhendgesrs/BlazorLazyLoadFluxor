using Fluxor;
using Host;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Shared.LazyLoad;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<IServiceFeature, ServiceFeature>();

builder.Services.AddFluxor(o => o.ScanAssemblies(typeof(Program).Assembly));

Dictionary<string, string> _lazyModulesToBeLoaded = new()
{
    { "lazy-load", "Feature1.wasm" },
    { "feature2", "Feature2.wasm" }
};

builder.Services.CustomServiceProviderBoostrap(_lazyModulesToBeLoaded);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var host = builder.Build();

await host.RunAsync();