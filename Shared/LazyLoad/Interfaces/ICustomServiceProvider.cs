using Microsoft.Extensions.DependencyInjection;

namespace Shared.LazyLoad;

public interface ICustomServiceProvider
{
    public IServiceProvider CurrentProvider { get; }
    void AddService(Action<IServiceCollection> configureServices);
    void UpdateProvider(IServiceProvider newProvider);
    IServiceProvider MergeServiceProviders(IServiceCollection newServices);
}