namespace Agenix.Screenplay.Tests.Shopping.Questions;

public class DeliveryCostQuestion : IQuestion<int>
{
    public Task<int> AnsweredBy(Actor actor)
    {
        return Task.FromResult(20);
    }
}
