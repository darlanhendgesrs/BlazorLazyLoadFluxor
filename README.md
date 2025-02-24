# **Blazor with Lazy Load and Fluxor**

**Overview**

BlazorLazyLoadFluxor is a Blazor WebAssembly application demonstrating the integration of lazy-loaded microfrontend modules with the Fluxor state management library. This approach allows for modular, scalable, and maintainable Blazor applications where features can be developed, loaded, and managed independently.

**Features**

 - *Microfrontend Architecture:* Dynamically load Blazor components as
   separate modules.
   
 - *Lazy Loading*: Load modules on demand to optimize application
   performance.
   
-   *Fluxor Integration*: Manage application state consistently across
   dynamically loaded modules.
   
 -  *Custom Dependency Injection*: Utilize a custom service provider to
   handle services in lazy-loaded modules.



# Getting started:

To get started with Lazy Loading in your general project, follow this tutorial to enable default lazy loading: 

https://learn.microsoft.com/en-us/aspnet/core/blazor/webassembly-lazy-load-assemblies?view=aspnetcore-9.0

# Getting into the real issue

Lazy loading works very well for simple components.
But what if your component uses a dependency injected service and you want that service to be lazy loaded as well??

Peter Himschoot describes the same problem that this library is trying to solve:  
[https://blogs.u2u.be/peter/post/blazor-lazy-loading-and-dependency-injection](https://blogs.u2u.be/peter/post/blazor-lazy-loading-and-dependency-injection)  However, at the critical point, he mentions:

> The way to fix this is to use a class that is not lazy loaded, and that will create the dependency after we have loaded the assembly.

You can also check more sources with approachs to solve the issue:

https://github.com/RonSijm/RonSijm.Blazyload


## Challenges & Why We Created a Custom Service Provider

When implementing lazy-loaded microfrontends in Blazor, we faced several challenges:

1.  **Dependency Injection (DI) Scope Issues**
    
    -   Blazor’s default `IServiceProvider` does not support modifying DI at runtime.
        
    -   Services registered after the app starts were not being picked up.
        
2.  **Managing Services Across Multiple Microfrontends**
    
    -   Each microfrontend module required its own services (e.g., Fluxor state, scoped services), which had to be dynamically registered.
        
    -   Without a proper DI solution, services from dynamically loaded modules were not accessible in the host application.


### Solution: CustomServiceProvider

To solve these issues, we implemented a **CustomServiceProvider**, which:

✅ **Dynamically registers services when a new module loads**  
✅ **Ensures Fluxor store updates with new reducers and states**  
✅ **Provides a flexible way to extend the DI container at runtime**  
✅ **Allows merging new services with existing services dynamically**


### Defining Lazy-Loaded Modules in `Program.cs`

To ensure the application knows which modules to load dynamically, we **declare the module paths and their respective** `**.wasm**` **assemblies** in the client's startup configuration:

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
    
    // Define lazy-loaded modules and their corresponding wasm files
    // the first parameter is the path and the second the Assembly.wasm (verion 8 and 9 ), before it was dll
    Dictionary<string, string> _lazyModulesToBeLoaded = new()
    {
        { "lazy-load", "Feature1.wasm" }
    };
    
    builder.Services.CustomServiceProviderBoostrap(_lazyModulesToBeLoaded);
    
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
    
    var host = builder.Build();
    
    await host.RunAsync();

By adding this dictionary (`_lazyModulesToBeLoaded`), we enable the system to dynamically load the correct WebAssembly module when a user navigates to a feature that isn't initially loaded.




### Attention: ###

In the following example where we declare:

    {
        { "lazy-load", "Feature1.wasm" }
    };


⚠️ **The path represents the wasm. So everything that comes after lazy-load/* will use Feature1.wasm.**


## App.razor file

### ⚠️ **Important: Custom Dependency Injection**

Since we are dynamically registering services using `CustomServiceProvider`, we **cannot use Blazor's conventional DI approach** (`[Inject]` or `builder.Services.AddScoped<T>()` during startup). Instead, we must **retrieve services via** `**CustomProvider.CurrentProvider**` like this:

```
var myService = CustomProvider.CurrentProvider.GetRequiredService<IMyService>();
```

With this approach, every dynamically loaded module can register its own services, including Fluxor states, without requiring pre-loading in the host application. Additionally, it allows merging new services dynamically without interfering with the existing DI container.

### 🔹 Handling Lazy Loading in `App.razor`

The **App.razor** file is responsible for dynamically loading modules when users navigate to different parts of the application. It integrates the `LazyAssemblyLoader` to load required assemblies and registers their services into the `CustomServiceProvider`.

    @using System.Reflection
    @using Microsoft.AspNetCore.Components.Routing
    @using Microsoft.AspNetCore.Components.WebAssembly.Services
    @using Microsoft.Extensions.DependencyInjection.Extensions
    @using Microsoft.Extensions.Logging
    @using Shared.LazyLoad
    
    @inject ILogger<App> Logger
    @inject ICustomServiceProvider CustomProvider
    
    <Router AppAssembly="@typeof(App).Assembly" AdditionalAssemblies="@LazyLoadModule.LoadedAssemblies" OnNavigateAsync="@OnNavigateAsync">
        <Found Context="routeData">
            <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
            <FocusOnNavigate RouteData="@routeData" Selector="h1" />
        </Found>
        <NotFound>
            <PageTitle>Not found</PageTitle>
            <LayoutView Layout="@typeof(MainLayout)">
                <p role="alert">Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
    
    @code {
        protected ILazyLoadModules LazyLoadModule { get; set; } = null!;
    
        protected override void OnInitialized()
        {
            LazyLoadModule = CustomProvider.CurrentProvider.GetRequiredService<ILazyLoadModules>();
        }
    
        private async Task OnNavigateAsync(NavigationContext args)
        {
            if (!string.IsNullOrWhiteSpace(args.Path))
                await LazyLoadModule.LoadModuleAsync(args.Path);
        }
    }

### Explanation of `App.razor`

-   **Dependency Injection**: Uses `ICustomServiceProvider` to resolve `ILazyLoadModules`.
    
-   **Dynamic Module Loading**: Calls `LazyLoadModule.LoadModuleAsync(args.Path)` on navigation.
    
-   **Maintains Router Configuration**: Ensures new modules are recognized in Blazor's routing system.

### What Happens When `LoadModuleAsync()` is Called?

When `LoadModuleAsync(path)` is executed, the following sequence occurs:

1️⃣ **Checks if the requested module should be lazy-loaded** based on predefined mappings in `LazyLoadVariables.AssembliesToBeLoaded`.

2️⃣ **Ensures the module isn't already loaded** by checking `LazyLoadVariables.AssembliesNameLoaded`.

3️⃣ **Loads the WebAssembly dynamically** using `AssemblyLoader.LoadAssembliesAsync(assemblyName)`, then stores it in `LoadedAssemblies`.

4️⃣ **Registers the module's services dynamically** by locating its `IModuleBootstrapper`, calling `ConfigureServices()`, and merging its services into the `CustomServiceProvider`.

5️⃣ **Updates Fluxor’s store to recognize new reducers and state** by calling `store.InitializeAsync()`.

This ensures that the new module is fully integrated into the application at runtime, without requiring a restart or pre-registration.

By using this approach, the app efficiently loads microfrontend modules only when needed, reducing initial load times and ensuring a scalable architecture.

Now, the application can dynamically load **Feature1.wasm** when the user navigates to the `lazy-load` route, and future modules can be added using the same approach.

