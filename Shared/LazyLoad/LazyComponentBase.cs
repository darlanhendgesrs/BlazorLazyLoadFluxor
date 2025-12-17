using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Shared.LazyLoad;

public abstract class LazyComponentBase : ComponentBase
{
    [Inject] 
    protected ICustomServiceProvider CustomProvider { get; set; } = null!;
    
    protected override void OnInitialized()
    {
        InjectLazyServices();
        base.OnInitialized();
    }
    
    private void InjectLazyServices()
    {
        var properties = GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(p => p.GetCustomAttribute<LazyInjectAttribute>() != null);
            
        foreach (var prop in properties)
        {
            var service = CustomProvider.CurrentProvider.GetRequiredService(prop.PropertyType);
            prop.SetValue(this, service);
        }
    }
    
    protected T GetService<T>() where T : notnull
        => CustomProvider.CurrentProvider.GetRequiredService<T>();
}
