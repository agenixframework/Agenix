using Agenix.Screenplay.Annotations;

namespace Agenix.Screenplay.Tests.Shopping.Tasks;

/// <summary>
///     The role of a task is to simulate a high-level action performed by the user.
///     It is common practice to add methods, e.g.
/// </summary>
public class PlaceInBasket : IPerformable
{
    [Step("And {0} has placed the item in her shopping basket")]
    public void PerformAs<T>(T actor) where T : Actor
    {
        // Implementation here
    }

    public static PlaceInBasket PlacedTheItemInHerBasket()
    {
        return Instrumented.InstanceOf<PlaceInBasket>().NewInstance();
    }
}
