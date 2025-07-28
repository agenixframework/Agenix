using Agenix.Screenplay.Annotations;

namespace Agenix.Screenplay.Tests.Shopping.Tasks;

public class ChewsTheBanana : IPerformable
{
    [Step("{0} chews the banana")]
    public void PerformAs<T>(T actor) where T : Actor
    {
        // Implementation here
    }
}
