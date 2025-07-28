using Agenix.Screenplay.Tests.Shopping.Exceptions;

namespace Agenix.Screenplay.Tests.Shopping.Questions;

public class ThankYouMessage : IQuestionDiagnostics, IQuestion<string>
{
    public string AnsweredBy(Actor actor)
    {
        return "Thank you!";
    }

    public Type OnError()
    {
        return typeof(PeopleAreSoRudeException);
    }

    public static ThankYouMessage TheThankYouMessage()
    {
        return new ThankYouMessage();
    }
}
