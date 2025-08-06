namespace Agenix.Screenplay.Tests.Shopping.Questions;

public class TotalCostIncludingDelivery : IQuestion<int>
{
    public Task<int> AnsweredBy(Actor actor)
    {
        return Task.FromResult(25);
    }

    public string Subject => "{0} asks for the total cost including delivery";

    public static TotalCostIncludingDelivery TheTotalCostIncludingDelivery()
    {
        return new TotalCostIncludingDelivery();
    }
}
