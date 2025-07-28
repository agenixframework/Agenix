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
    public void ShouldBeAbleToPurchaseSomeItemsWithDelivery()
    {
        GivenThat(_dana).Has(Purchased().AnApple().ThatCosts(10).Dollars(),
            AndPurchased().APear().ThatCosts(5).Dollars());

        When(_dana).AttemptsTo(HaveItemsDelivered.Now());

        Then(_dana).Should(SeeThat(TheCorrectTotalCost(), EqualTo(15)),
            SeeThat(TheTotalCostIncludingDelivery(), GreaterThanOrEqualTo(20)));
        Then(_dana).Should(SeeThat(TheThankYouMessage(), EqualTo("Thank you!")));
    }

    [Test]
    public void ShouldBeAbleToPurchaseSomeItemsWithDeliveryUsingPredicates()
    {
        GivenThat(_dana).Has(Purchased().AnApple().ThatCosts(10).Dollars(),
            AndPurchased().APear().ThatCosts(5).Dollars());

        When(_dana).AttemptsTo(HaveItemsDelivered.Now());

        Then(_dana).Should(SeeThat("Total cost", TheCorrectTotalCost(),
            ReturnsAValueThat<int>("is equal to 15", value => value == 15)));
    }

    [Test]
    public void ShouldBeAbleToPurchaseSomeItemsWithDeliveryAndCustomErrors()
    {
        GivenThat(_dana).Has(Purchased().AnApple().ThatCosts(10).Dollars(),
            AndPurchased().APear().ThatCosts(5).Dollars());

        When(_dana).AttemptsTo(HaveItemsDelivered.Now());

        Then(_dana).Should(SeeThat(TheTotalCostIncludingDelivery(), GreaterThanOrEqualTo(20)));
        Assert.Throws<AgenixSystemException>(() =>
            Then(_dana).Should(SeeThat(TheThankYouMessage(), EqualTo("You're welcome"))));
    }

    [Test]
    public void ShouldBeAbleToPurchaseSomeItemsWithDeliveryAndCustomErrorsByQuestion()
    {
        GivenThat(_dana).Has(Purchased().AnApple().ThatCosts(10).Dollars(),
            AndPurchased().APear().ThatCosts(5).Dollars());

        When(_dana).AttemptsTo(HaveItemsDelivered.Now());

        Then(_dana).Should(SeeThat(TheTotalCostIncludingDelivery(), GreaterThanOrEqualTo(20)));
        Assert.Throws<PeopleAreSoImpoliteException>(() => Then(_dana).Should(
            SeeThat(TheThankYouMessage(), EqualTo("You're welcome"))
                .OrComplainWith(typeof(PeopleAreSoImpoliteException))));
    }

    [Test]
    public void ShouldBeAbleToPurchaseSomeItemsWithDeliveryAndCustomErrorsByQuestionWithACustomMessage()
    {
        GivenThat(_dana).Has(Purchased().AnApple().ThatCosts(10).Dollars(),
            AndPurchased().APear().ThatCosts(5).Dollars());

        When(_dana).AttemptsTo(HaveItemsDelivered.Now());

        Then(_dana).Should(SeeThat(TheTotalCostIncludingDelivery(), GreaterThanOrEqualTo(20)));

        Assert.Throws<PeopleAreSoImpoliteException>(() => Then(_dana).Should(
            SeeThat(TheThankYouMessage(), EqualTo("You're welcome"))
                .OrComplainWith(typeof(PeopleAreSoImpoliteException), "You should say something nice")));
    }


    [Test]
    public void ShouldBeAbleToAskForNiceThings()
    {
        var totalCost = _dana.AsksFor(TheTotalCost());
        Assert.That(totalCost, Is.EqualTo(14));
    }

    [Test]
    public void ShouldBeAbleToRememberAnswersToQuestions()
    {
        _dana.Remember("Total Cost", TheTotalCost());
        var totalCost = _dana.Recall<int>("Total Cost");
        Assert.That(totalCost, Is.EqualTo(14));
    }

    [Test]
    public void ShouldBeAbleToRememberValues()
    {
        _dana.Remember("Total Cost", 14);
        Assert.That(_dana.Recall<int>("Total Cost"), Is.EqualTo(14));

        var colorSet = new List<string> { "red", "green", "blue" };
        Assert.That(colorSet, Is.All.AnyOf
            ("red", "green", "blue", "yellow"));
    }

    [Test]
    public void ShouldBeAbleToPurchaseSomeItems()
    {
        GivenThat(_dana).Has(Purchased().AnApple().ThatCosts(10).Dollars(),
            Purchased().APear().ThatCosts(5).Dollars());

        Assert.DoesNotThrow(() => When(_dana).AttemptsTo(HaveItemsDelivered.Now()));
    }


    [Test]
    public void ShouldBeAbleToWaitInCheckoutLine()
    {
        var fastCheckout = Checkout.FastCheckout();

        GivenThat(_dana).AttemptsTo(JoinTheCheckoutQueue().Of(fastCheckout));

        Then(_dana).Should(Eventually(SeeThat(
                By(fastCheckout), EqualTo("Dana")
            )).WaitingForNoLongerThan(3).Seconds()
            .OrComplainWith(typeof(ThisTakesTooLongException)));
    }

    [Test]
    public void ShouldPatientlyWaitInCheckoutLine()
    {
        var slowCheckout = Checkout.SlowCheckout();

        GivenThat(_dana).AttemptsTo(JoinTheCheckoutQueue().Of(slowCheckout));

        Then(_dana).Should(Eventually(SeeThat(
                By(slowCheckout), EqualTo("Dana")
            )).WaitingForNoLongerThan(10).Seconds()
            .OrComplainWith(typeof(ThisTakesTooLongException)));
    }

    [Test]
    public void ShouldPatientlyWaitThenPurchaseItems()
    {
        var slowCheckout = Checkout.SlowCheckout();

        Assert.DoesNotThrow(() => GivenThat(_dana).AttemptsTo(
            JoinTheCheckoutQueue().Of(slowCheckout),
            Wait.Until(By(slowCheckout), EqualTo("Dana"))
                .ForNoMoreThan(10).Seconds(),
            Purchase().AnApple().ThatCosts(10).Dollars()
        ));
    }

    [Test]
    public void ShouldImpatientlyWaitInCheckoutLine()
    {
        var fastCheckout = Checkout.FastCheckout();

        GivenThat(_dana).AttemptsTo(JoinTheCheckoutQueue().Of(fastCheckout));

        Assert.Throws<ThisTakesTooLongException>(() =>
            Then(_dana).Should(Eventually(SeeThat(
                    By(fastCheckout), EqualTo("Dana")
                )).WaitingForNoLongerThan(1).Seconds()
                .OrComplainWith(typeof(ThisTakesTooLongException)))
        );
    }
}
