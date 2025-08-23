using Agenix.Screenplay.Tests.Shopping.Exceptions;
using static Agenix.Screenplay.GivenWhenThen;
using static Agenix.Screenplay.Tests.Shopping.Questions.ThankYouMessage;
using static NHamcrest.Is;

namespace Agenix.Screenplay.Tests.Shopping.Questions;

public class NestedThankYouMessage : IQuestionDiagnostics, IQuestion<string>
{
    public async Task<string> AnsweredBy(Actor actor)
    {
        await actor.Should(
            SeeThat(TheThankYouMessage(), EqualTo("You're welcome")),
            SeeThat(TheThankYouMessage(), EqualTo("No problem")),
            SeeThat(TheThankYouMessage(), EqualTo("Thank you!"))
        );
        return await TheThankYouMessage().AnsweredBy(actor);
    }

    public Type OnError()
    {
        return typeof(PeopleAreSoRudeException);
    }

    public static NestedThankYouMessage TheNestedThankYouMessage()
    {
        return new NestedThankYouMessage();
    }
}
