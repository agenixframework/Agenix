using Agenix.Api.Exceptions;
using Agenix.Screenplay.Tests.Shopping.Exceptions;
using Agenix.Screenplay.Tests.Shopping.Tasks;
using NUnit.Framework;
using static Agenix.Screenplay.GivenWhenThen;
using static Agenix.Screenplay.Tests.Shopping.Questions.NextPersonToBeServedQuestion;
using static Agenix.Screenplay.Tests.Shopping.Questions.ThankYouMessage;
using static Agenix.Screenplay.Tests.Shopping.Questions.TotalCost;
using static Agenix.Screenplay.Tests.Shopping.Questions.TotalCostIncludingDelivery;
using static Agenix.Screenplay.Tests.Shopping.Tasks.JoinTheCheckoutQueueTask;
using static Agenix.Screenplay.Tests.Shopping.Tasks.PurchaseItem;
using static NHamcrest.Is;
using Wait = Agenix.Screenplay.Questions.Waits.Wait;


namespace Agenix.Screenplay.Tests.Shopping;

[TestFixture]
public class DanaGoesShoppingSample
{
    private readonly Actor _dana = Actor.Named("Dana");

    [Test]
    public async Task ShouldBeAbleToPurchaseSomeItemsWithDelivery()
    {
        await GivenThat(_dana).Has(Purchased().AnApple().ThatCosts(10).Dollars(),
            AndPurchased().APear().ThatCosts(5).Dollars());

        await When(_dana).AttemptsTo(HaveItemsDelivered.Now());

        await Then(_dana).Should(SeeThat(TheCorrectTotalCost(), EqualTo(15)),
            SeeThat(TheTotalCostIncludingDelivery(), GreaterThanOrEqualTo(20)));
        await Then(_dana).Should(SeeThat(TheThankYouMessage(), EqualTo("Thank you!")));
    }

    [Test]
    public async Task ShouldBeAbleToPurchaseSomeItemsWithDeliveryUsingPredicates()
    {
        await GivenThat(_dana).Has(Purchased().AnApple().ThatCosts(10).Dollars(),
            AndPurchased().APear().ThatCosts(5).Dollars());

        await When(_dana).AttemptsTo(HaveItemsDelivered.Now());

        await Then(_dana).Should(SeeThat("Total cost", TheCorrectTotalCost(),
            ReturnsAValueThat<int>("is equal to 15", value => value == 15)));
    }

    [Test]
    public async Task ShouldBeAbleToPurchaseSomeItemsWithDeliveryAndCustomErrors()
    {
        await GivenThat(_dana).Has(Purchased().AnApple().ThatCosts(10).Dollars(),
            AndPurchased().APear().ThatCosts(5).Dollars());

        await When(_dana).AttemptsTo(HaveItemsDelivered.Now());

        await Then(_dana).Should(SeeThat(TheTotalCostIncludingDelivery(), GreaterThanOrEqualTo(20)));
        Assert.ThrowsAsync<AgenixSystemException>(() =>
            Then(_dana).Should(SeeThat(TheThankYouMessage(), EqualTo("You're welcome"))));
    }

    [Test]
    public async Task ShouldBeAbleToPurchaseSomeItemsWithDeliveryAndCustomErrorsByQuestion()
    {
        await GivenThat(_dana).Has(Purchased().AnApple().ThatCosts(10).Dollars(),
            AndPurchased().APear().ThatCosts(5).Dollars());

        await When(_dana).AttemptsTo(HaveItemsDelivered.Now());

        await Then(_dana).Should(SeeThat(TheTotalCostIncludingDelivery(), GreaterThanOrEqualTo(20)));
        Assert.ThrowsAsync<PeopleAreSoImpoliteException>(() => Then(_dana).Should(
            SeeThat(TheThankYouMessage(), EqualTo("You're welcome"))
                .OrComplainWith(typeof(PeopleAreSoImpoliteException))));
    }

    [Test]
    public async Task ShouldBeAbleToPurchaseSomeItemsWithDeliveryAndCustomErrorsByQuestionWithACustomMessage()
    {
        await GivenThat(_dana).Has(Purchased().AnApple().ThatCosts(10).Dollars(),
            AndPurchased().APear().ThatCosts(5).Dollars());

        await When(_dana).AttemptsTo(HaveItemsDelivered.Now());

        await Then(_dana).Should(SeeThat(TheTotalCostIncludingDelivery(), GreaterThanOrEqualTo(20)));

        Assert.ThrowsAsync<PeopleAreSoImpoliteException>(() => Then(_dana).Should(
            SeeThat(TheThankYouMessage(), EqualTo("You're welcome"))
                .OrComplainWith(typeof(PeopleAreSoImpoliteException), "You should say something nice")));
    }


    [Test]
    public async Task ShouldBeAbleToAskForNiceThings()
    {
        var totalCost = await _dana.AsksFor(TheTotalCost());
        Assert.That(totalCost, Is.EqualTo(14));
    }

    [Test]
    public async Task ShouldBeAbleToRememberAnswersToQuestions()
    {
        await _dana.Remember("Total Cost", TheTotalCost());
        var totalCost = await _dana.Recall<int>("Total Cost");
        Assert.That(totalCost, Is.EqualTo(14));
    }

    [Test]
    public async Task ShouldBeAbleToRememberValues()
    {
        await _dana.Remember("Total Cost", 14);
        Assert.That(await _dana.Recall<int>("Total Cost"), Is.EqualTo(14));

        var colorSet = new List<string> { "red", "green", "blue" };
        Assert.That(colorSet, Is.All.AnyOf
            ("red", "green", "blue", "yellow"));
    }

    [Test]
    public async Task ShouldBeAbleToPurchaseSomeItems()
    {
        await GivenThat(_dana).Has(Purchased().AnApple().ThatCosts(10).Dollars(),
            Purchased().APear().ThatCosts(5).Dollars());

        Assert.DoesNotThrow(() => When(_dana).AttemptsTo(HaveItemsDelivered.Now()));
    }


    [Test]
    public async Task ShouldBeAbleToWaitInCheckoutLine()
    {
        var fastCheckout = Checkout.FastCheckout();

        await GivenThat(_dana).AttemptsTo(JoinTheCheckoutQueue().Of(fastCheckout));

        await Then(_dana).Should(Eventually(SeeThat(
                By(fastCheckout), EqualTo("Dana")
            )).WaitingForNoLongerThan(3).Seconds()
            .OrComplainWith(typeof(ThisTakesTooLongException)));
    }

    [Test]
    public async Task ShouldPatientlyWaitInCheckoutLine()
    {
        var slowCheckout = Checkout.SlowCheckout();

        await GivenThat(_dana).AttemptsTo(JoinTheCheckoutQueue().Of(slowCheckout));

        await Then(_dana).Should(Eventually(SeeThat(
                By(slowCheckout), EqualTo("Dana")
            )).WaitingForNoLongerThan(10).Seconds()
            .OrComplainWith(typeof(ThisTakesTooLongException)));
    }

    [Test]
    public void ShouldPatientlyWaitThenPurchaseItems()
    {
        var slowCheckout = Checkout.SlowCheckout();

        Assert.DoesNotThrowAsync(() => GivenThat(_dana).AttemptsTo(
            JoinTheCheckoutQueue().Of(slowCheckout),
            Wait.Until(By(slowCheckout), EqualTo("Dana"))
                .ForNoMoreThan(10).Seconds(),
            Purchase().AnApple().ThatCosts(10).Dollars()
        ));
    }

    [Test]
    public async Task ShouldImpatientlyWaitInCheckoutLine()
    {
        var fastCheckout = Checkout.FastCheckout();

        await GivenThat(_dana).AttemptsTo(JoinTheCheckoutQueue().Of(fastCheckout));

        Assert.ThrowsAsync<ThisTakesTooLongException>(() =>
            Then(_dana).Should(Eventually(SeeThat(
                    By(fastCheckout), EqualTo("Dana")
                )).WaitingForNoLongerThan(1).Seconds()
                .OrComplainWith(typeof(ThisTakesTooLongException)))
        );
    }
}
