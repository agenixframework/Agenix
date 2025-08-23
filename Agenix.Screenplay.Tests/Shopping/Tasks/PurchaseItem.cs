using Agenix.Screenplay.Annotations;
using NUnit.Framework;
using static Agenix.Screenplay.GivenWhenThen;
using static Agenix.Screenplay.Tests.Shopping.Tasks.PlaceInBasket;

namespace Agenix.Screenplay.Tests.Shopping.Tasks;

/// <summary>
///     The role of a task is to simulate a high-level action performed by the user.
///     It is common practice to add methods, e.g.
/// </summary>
public class PurchaseItem(string purchasedItem, int cost, string currency) : IPerformable
{
    private int _cost = cost;
    private string _currency = currency;
    private string _purchasedItem = purchasedItem;

    public PurchaseItem() : this(null, 0, null)
    {
    }

    public PurchaseItem(string purchasedItem, string currency) : this(purchasedItem, 0, currency)
    {
    }

    public bool TheItemWasPurchased { get; set; }

    public string Currency { get; private set; } = currency;

    [Step("Given {0} has purchased #_purchasedItem for #_cost #_currency")]
    public async Task PerformAsAsync<T>(T actor, CancellationToken cancellationToken = default) where T : Actor
    {
        Assert.That(_purchasedItem, Is.Not.Null);
        Assert.That(_currency, Is.Not.Null);
        Assert.That(_cost, Is.GreaterThan(0));
        await AndThat(actor).Has(PlacedTheItemInHerBasket());

        TheItemWasPurchased = true;
    }

    public static PurchaseItem Purchased()
    {
        return Instrumented.InstanceOf<PurchaseItem>().NewInstance();
    }

    public static PurchaseItem Purchase()
    {
        return Instrumented.InstanceOf<PurchaseItem>().NewInstance();
    }

    public static PurchaseItem AndPurchased()
    {
        return Instrumented.InstanceOf<PurchaseItem>().NewInstance();
    }

    public PurchaseItem AnApple()
    {
        _purchasedItem = "an apple";
        return this;
    }

    public PurchaseItem APear()
    {
        _purchasedItem = "a pear";
        return this;
    }

    public PurchaseItem ThatCosts(int newCost)
    {
        _cost = newCost;
        if (_cost == 0)
        {
            throw new AssertionException("Cost must be greater than 0");
        }

        return _cost < 0 ? throw new ArgumentException("Cost cannot be negative") : this;
    }

    public PurchaseItem Dollars()
    {
        _currency = "dollars";
        return this;
    }
}
