using Fluxor;
using Microsoft.AspNetCore.Components.WebAssembly.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Shared.LazyLoad;

public class LazyLoadModules(ICustomServiceProvider CustomProvider, LazyAssemblyLoader AssemblyLoader, IConfiguration Configuration, ILogger<ILazyLoadModules> Logger) : ILazyLoadModules
{
    public List<Assembly> LoadedAssemblies { get; private set; } = new();

    public async Task LoadModuleAsync(string path)
    {
        try
        {
            if (path.IndexOf("/") > 0)
                path = path.Split("/").First();

            var assemblyName = LazyLoadVariables.AssembliesToBeLoaded.GetValueOrDefault(path.ToLower());

            if (assemblyName == null || LazyLoadVariables.AssembliesNameLoaded.Any(l => l == assemblyName))
                return;

            await LoadAssemblyAsync(assemblyName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Lazy Load LoadModuleAsync: {ex.Message}");
        }
    }
    private async Task LoadAssemblyAsync(string assemblyName)
    {
        var assemblies = await AssemblyLoader.LoadAssembliesAsync(new[] { assemblyName });
        
        LoadedAssemblies.AddRange(assemblies);
        
        LazyLoadVariables.AssembliesNameLoaded.Add(assemblyName);
        
        foreach (var assembly in assemblies)
            RegisterModuleServices(assembly);

    }
    private void RegisterModuleServices(Assembly assembly)
    {
        var bootstrapperType = assembly.GetTypes()
            .FirstOrDefault(t => typeof(IModuleBootstrapper).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        if (bootstrapperType == null)
            return;

        var moduleBootstrapper = (IModuleBootstrapper)Activator.CreateInstance(bootstrapperType, CustomProvider)!;

        var _dynamicServices = new ServiceCollection();

        moduleBootstrapper.ConfigureServices(_dynamicServices, Configuration);

        CustomProvider.UpdateProvider(CustomProvider.MergeServiceProviders(_dynamicServices));

        var provider = CustomProvider.CurrentProvider;
        
        var store = provider.GetRequiredService<IStore>();

        store.InitializeAsync();
    }
}
