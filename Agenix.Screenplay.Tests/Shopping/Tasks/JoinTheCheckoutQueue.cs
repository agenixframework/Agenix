namespace Agenix.Screenplay.Tests.Shopping.Tasks;

public class JoinTheCheckoutQueueTask : IPerformable
{
    public Task PerformAsAsync<T>(T actor, CancellationToken cancellationToken = default) where T : Actor
    {
        return Task.CompletedTask;
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
