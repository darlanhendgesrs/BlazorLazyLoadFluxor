using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Shared.LazyLoad;

namespace Feature1;

public class Feature1Bootstrapper(CustomServiceProvider CustomProvider) : IModuleBootstrapper
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        CustomProvider.AddService(s =>
        {
            s.AddFluxor(o => o.ScanAssemblies(typeof(Feature1Bootstrapper).Assembly));
            s.AddScoped<IServiceFeature1, ServiceFeature1>();
        });
    }
}
