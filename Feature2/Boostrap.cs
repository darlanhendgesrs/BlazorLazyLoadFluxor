using Feature2;
using Fluxor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.LazyLoad;

namespace Feature2;

public class Feature1Bootstrapper(CustomServiceProvider CustomProvider) : IModuleBootstrapper
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        CustomProvider.AddService(s =>
        {
            s.AddFluxor(o => o.ScanAssemblies(typeof(Feature1Bootstrapper).Assembly));
            s.AddScoped<IServiceFeature2, ServiceFeature2>();
        });
    }
}
