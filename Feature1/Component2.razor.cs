using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Fluxor;
using Feature1.State;
using Shared.LazyLoad;

namespace Feature1;

public partial class Component2 : LazyComponentBase
{
    [Inject] private ILogger<Component2> Logger { get; set; } = null!;

    [LazyInject]
    private IDispatcher Dispatcher { get; set; } = null!;

    [LazyInject]
    private IState<Feature1State> State { get; set; } = null!;

    [LazyInject]
    private IServiceFeature1 ServiceFeature1 { get; set; } = null!;

    private void IncrementCounter()
    {
        Dispatcher.Dispatch(new IncrementFeature1CounterAction());
        ServiceFeature1.Test();
    }
}
