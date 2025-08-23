using Agenix.Screenplay.Annotations;

namespace Agenix.Screenplay.Tests.Shopping.Tasks;

public class HaveItemsDelivered : IPerformable
{
    [Step("And {0} has them delivered")]
    public Task PerformAsAsync<T>(T actor, CancellationToken cancellationToken = default) where T : Actor
    {
        return Task.CompletedTask;
    }

    public static HaveItemsDelivered Now()
    {
        return new HaveItemsDelivered();
    }
}
