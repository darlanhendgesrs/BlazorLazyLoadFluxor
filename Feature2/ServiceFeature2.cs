namespace Feature2;

public interface IServiceFeature2
{
    public void Test();
}

public class ServiceFeature2 : IServiceFeature2
{
    public void Test()
    {
        Console.WriteLine($"ServiceFeature2 using DI");
    }
}
