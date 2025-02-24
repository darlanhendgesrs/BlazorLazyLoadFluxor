using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Shared.LazyLoad; 

public class CustomServiceProvider : ICustomServiceProvider
{
    private IServiceProvider _currentProvider;
    private readonly IServiceCollection _services;
    private IServiceCollection Services => _services;
    public IServiceProvider CurrentProvider => _currentProvider;

    public CustomServiceProvider(IServiceProvider initialProvider, IServiceCollection? originalServices = null)
    {
        _currentProvider = initialProvider;
        _services = originalServices ?? new ServiceCollection();
    }

    public void AddService(Action<IServiceCollection> configureServices)
    {
        configureServices(_services);

        var factory = new DefaultServiceProviderFactory();
        _currentProvider = factory.CreateServiceProvider(_services);

        Console.WriteLine("CustomServiceProvider updated with new services.");
    }

    public void UpdateProvider(IServiceProvider newProvider)
    {
        _currentProvider = newProvider;
        Console.WriteLine("CustomServiceProvider has been updated globally.");
    }

    public IServiceProvider MergeServiceProviders(IServiceCollection newServices)
    {
        var mergedServices = new ServiceCollection();

        foreach (var service in Services)
            mergedServices.Add(service);

        foreach (var service in newServices)
            mergedServices.Add(service);

        return mergedServices.BuildServiceProvider();
    }
}
