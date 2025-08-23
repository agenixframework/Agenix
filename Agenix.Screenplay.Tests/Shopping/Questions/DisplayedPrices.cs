using Agenix.Screenplay.Annotations;
using static Agenix.Screenplay.GivenWhenThen;
using static NHamcrest.Is;

namespace Agenix.Screenplay.Tests.Shopping.Questions;

[Subject("the prices should be correctly displayed")]
public class DisplayedPrices(int vat, bool throwError) : IQuestion<object>
{
    public async Task<object> AnsweredBy(Actor actor)
    {
        await actor.Should(
            SeeThat("the total price", ThePrice.Total(), EqualTo(100)),
            SeeThat("the VAT", ThePrice.Vat(throwError), EqualTo(vat)),
            SeeThat("the price with VAT", ThePrice.TotalWithVat(), EqualTo(120))
        );
        return null;
    }

    public static DisplayedPrices ThePriceIsCorrectlyDisplayed()
    {
        return new DisplayedPrices(20, false);
    }

    public static DisplayedPrices ThePriceIsIncorrectlyDisplayed()
    {
        return new DisplayedPrices(30, false);
    }

    public static DisplayedPrices ThePriceIsIncorrectlyDisplayedWithAnError()
    {
        return new DisplayedPrices(20, true);
    }
}
