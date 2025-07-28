using Agenix.Screenplay.Annotations;

namespace Agenix.Screenplay.Tests.Shopping.Tasks;

public class PeelABanana : IPerformable
{
    [Step("{0} peels a banana")]
    public void PerformAs<T>(T actor) where T : Actor
    {
        // Implementation here
        Console.WriteLine($"{actor.Name} peels a banana");
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
