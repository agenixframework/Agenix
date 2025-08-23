using Agenix.Screenplay.Annotations;

namespace Agenix.Screenplay.Tests.Shopping.Tasks;

public class PeelABanana : IPerformable
{
    [Step("{0} peels a banana")]
    public Task PerformAsAsync<T>(T actor, CancellationToken cancellationToken = default) where T : Actor
    {
        // Implementation here
        Console.WriteLine($"{actor.Name} peels a banana");

        return Task.CompletedTask;
    }

    public static PeelABanana Now()
    {
        return Instrumented.InstanceOf<PeelABanana>().NewInstance();
    }

    public override string ToString()
    {
        return "PeelABanana";
    }
}
