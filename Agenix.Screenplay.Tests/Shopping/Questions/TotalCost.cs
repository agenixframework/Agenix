namespace Agenix.Screenplay.Tests.Shopping.Questions;

public class TotalCost(int total) : IQuestion<int>
{
    public int AnsweredBy(Actor actor)
    {
        return total;
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
