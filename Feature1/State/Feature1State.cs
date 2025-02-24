using Fluxor;

namespace Feature1.State;

public record IncrementFeature1CounterAction;

public class Feature1Reducers
{

    [ReducerMethod]
    public Feature1State ReduceIncrement(Feature1State state, IncrementFeature1CounterAction action)
    {
        return new Feature1State(state.Counter + 1);
    }
}

[FeatureState]
public class Feature1State
{
    public int Counter { get; }



    public Feature1State() : this(0)
    {

    }

    public Feature1State(int counter)
    {
        Counter = counter;
    }
}