namespace Feature1;

public interface IServiceFeature1
{
    public void Test();
}

public class ServiceFeature1 : IServiceFeature1
{
    public void Test()
    {
        Console.WriteLine($"ServiceFeature1 using DI");
    }
}
