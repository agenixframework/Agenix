namespace Agenix.Screenplay.Tests.Shopping.Questions;

public class TotalCost(int total) : IQuestion<int>
{
    public Task<int> AnsweredBy(Actor actor)
    {
        return Task.FromResult(total);
    }

    public static TotalCost TheTotalCost()
    {
        return new TotalCost(14);
    }

    public static TotalCost TheCorrectTotalCost()
    {
        return new TotalCost(15);
    }
}
