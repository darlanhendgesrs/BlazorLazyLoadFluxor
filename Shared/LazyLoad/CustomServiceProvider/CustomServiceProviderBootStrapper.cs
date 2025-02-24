using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Shared.LazyLoad;

public static class CustomServiceProviderBootStrapper
{
    public static void CustomServiceProviderBoostrap(this IServiceCollection serviceCollection, Dictionary<string, string> lazyModulesToBeLoaded)
    {
        LazyLoadVariables.AssembliesToBeLoaded = lazyModulesToBeLoaded;

        var originalServices = new ServiceCollection();
        foreach (var service in serviceCollection)
            originalServices.TryAdd(service);

        serviceCollection.AddSingleton<ICustomServiceProvider>(sp => new CustomServiceProvider(sp, originalServices));
        serviceCollection.AddSingleton<ILazyLoadModules, LazyLoadModules>();
    }
}
