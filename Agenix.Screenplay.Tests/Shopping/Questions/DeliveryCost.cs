namespace Agenix.Screenplay.Tests.Shopping.Questions;

public class DeliveryCostQuestion : IQuestion<int>
{
    public int AnsweredBy(Actor actor)
    {
        return 20;
    }
}
