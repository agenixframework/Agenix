using Agenix.Screenplay.Tests.Shopping.Tasks;

namespace Agenix.Screenplay.Tests.Shopping.Questions;

public class NextPersonToBeServedQuestion : IQuestion<string>
{
    private readonly Checkout? _checkout;

    private NextPersonToBeServedQuestion(Checkout? checkout)
    {
        _checkout = checkout;
    }

    public string AnsweredBy(Actor actor)
    {
        if (_checkout == null)
        {
            // Default behavior - perhaps get default checkout from actor's abilities
            throw new InvalidOperationException("No checkout specified. Use By(checkout) to specify which checkout.");
        }

        return _checkout.NextCustomer();
    }

    public string Subject => "{0} asks for the next person to be served";

    public static NextPersonToBeServedQuestion NextPersonToBeServed()
    {
        return new NextPersonToBeServedQuestion(null);
    }

    public static NextPersonToBeServedQuestion By(Checkout checkout)
    {
        return new NextPersonToBeServedQuestion(checkout);
    }
}
