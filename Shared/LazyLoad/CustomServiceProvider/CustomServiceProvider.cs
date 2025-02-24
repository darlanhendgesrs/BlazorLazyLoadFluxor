using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Shared.LazyLoad; 

public class CustomServiceProvider(IServiceProvider InitialProvider, IServiceCollection? OriginalServices = null) : ICustomServiceProvider
{
    private IServiceProvider _currentProvider = InitialProvider;
    private readonly IServiceCollection _services = OriginalServices ?? new ServiceCollection();
    private IServiceCollection Services => _services;
    public IServiceProvider CurrentProvider => _currentProvider;

    public void AddService(Action<IServiceCollection> configureServices)
    {
        configureServices(_services);

        var factory = new DefaultServiceProviderFactory();
        _currentProvider = factory.CreateServiceProvider(_services);
    }

    public void UpdateProvider(IServiceProvider newProvider)
    {
        _currentProvider = newProvider;
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
