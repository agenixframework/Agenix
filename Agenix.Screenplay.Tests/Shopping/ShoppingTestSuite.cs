using Agenix.Screenplay.Tests.Shopping.Tasks;
using NUnit.Framework;
using static Agenix.Screenplay.GivenWhenThen;
using static Agenix.Screenplay.Tests.Shopping.Questions.ThankYouMessage;
using static Agenix.Screenplay.Tests.Shopping.Questions.TotalCost;
using static Agenix.Screenplay.Tests.Shopping.Questions.TotalCostIncludingDelivery;
using static Agenix.Screenplay.Tests.Shopping.Tasks.PurchaseItem;
using static NHamcrest.Is;


namespace Agenix.Screenplay.Tests.Shopping;

[TestFixture]
public class DanaGoesShoppingQuicklySample
{
    [SetUp]
    public void SetUp()
    {
        // Initialize steps if needed
    }

    private readonly Actor _dana = Actor.Named("Dana");

    [Test]
    public async Task ShouldBeAbleToPurchaseSomeItemsWithDelivery()
    {
        await GivenThat(_dana).Has(Purchased().AnApple().ThatCosts(10).Dollars(),
            AndPurchased().APear().ThatCosts(5).Dollars());

        await When(_dana).AttemptsTo(HaveItemsDelivered.Now());

        await Then(_dana).Should(SeeThat(TheCorrectTotalCost(), EqualTo(15)),
            SeeThat("The total cost including delivery", TheTotalCostIncludingDelivery(), GreaterThanOrEqualTo(20)));

        await Then(_dana).Should(SeeThat(TheThankYouMessage(), EqualTo("Thank you!")));
    }

    [Test]
    public async Task ShouldBeAbleToPurchaseSomeItems()
    {
        await GivenThat(_dana).Has(Purchased().AnApple().ThatCosts(10).Dollars(),
            Purchased().APear().ThatCosts(5).Dollars());

        Assert.DoesNotThrowAsync(() => When(_dana).AttemptsTo(HaveItemsDelivered.Now()));
    }

    // Expected to fail
    [Test]
    public async Task ShouldBeAbleToPurchaseAnItemForFree()
    {
        Assert.Throws<AssertionException>(() => GivenThat(_dana).AttemptsTo(Purchase().AnApple().ThatCosts(0).Dollars(),
            Purchase().APear().ThatCosts(5).Dollars()));
        await Then(_dana).Should(SeeThat(TheTotalCost(), EqualTo(14)));
    }

    // Expected to fail with an error
    [Test]
    public void ShouldBeAbleToPurchaseAnItemWithANegativeAmount()
    {
        Assert.ThrowsAsync<ArgumentException>(() => GivenThat(_dana).AttemptsTo(
            Purchase().AnApple().ThatCosts(-10).Dollars(), // Will fail with an error
            Purchase().APear().ThatCosts(5).Dollars())); // Should be skipped
    }
}
