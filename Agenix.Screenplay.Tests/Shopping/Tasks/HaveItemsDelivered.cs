using Agenix.Screenplay.Annotations;

namespace Agenix.Screenplay.Tests.Shopping.Tasks;

public class HaveItemsDelivered : IPerformable
{
    [Step("And {0} has them delivered")]
    public void PerformAs<T>(T actor) where T : Actor
    {
    }

    public static HaveItemsDelivered Now()
    {
        return new HaveItemsDelivered();
    }
}
