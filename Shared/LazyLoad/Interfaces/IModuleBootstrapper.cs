using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.LazyLoad;

public interface IModuleBootstrapper
{
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}
