using Agenix.Screenplay.Annotations;

namespace Agenix.Screenplay.Tests.Shopping.Tasks;

public class BitesTheBanana : IPerformable
{
    [Step("{0} bites the banana")]
    public Task PerformAsAsync<T>(T actor, CancellationToken cancellationToken = default) where T : Actor
    {
        return Task.CompletedTask;
    }
}
