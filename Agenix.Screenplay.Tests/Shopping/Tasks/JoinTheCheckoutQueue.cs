namespace Agenix.Screenplay.Tests.Shopping.Tasks;

public class JoinTheCheckoutQueueTask : IPerformable
{
    public void PerformAs<T>(T actor) where T : Actor
    {
        // Implementation here
    }

    public static JoinTheCheckoutQueueTask JoinTheCheckoutQueue()
    {
        return Instrumented.InstanceOf<JoinTheCheckoutQueueTask>().NewInstance();
    }

    public JoinTheCheckoutQueueTask Of(Checkout c)
    {
        return Instrumented.InstanceOf<JoinTheCheckoutQueueTask>().WithProperties(c);
    }
}
