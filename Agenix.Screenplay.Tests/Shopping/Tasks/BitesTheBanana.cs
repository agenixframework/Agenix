using Agenix.Screenplay.Annotations;

namespace Agenix.Screenplay.Tests.Shopping;

public class BitesTheBanana : IPerformable
{
    [Step("{0} bites the banana")]
    public void PerformAs<T>(T actor) where T : Actor
    {
        // Implementation here
    }
}
