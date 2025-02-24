using System.Reflection;

namespace Shared.LazyLoad;

public interface ILazyLoadModules
{
    public List<Assembly> LoadedAssemblies { get; }
    Task LoadModuleAsync(string path);
}