namespace Host;

public interface IServiceFeature
{
    public void Test();
}

public class ServiceFeature : IServiceFeature
{
    public void Test()
    {
        Console.WriteLine("Testing...");
    }
}
