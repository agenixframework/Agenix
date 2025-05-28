namespace Agenix.Screenplay.Questions;

/// <summary>
/// A question that calculates the sum of a collection of integers.
/// </summary>
public class SumQuestion(IQuestion<ICollection<int>> listQuestion) : IQuestion<int>
{
    public int AnsweredBy(Actor actor)
    {
        return listQuestion.AnsweredBy(actor).Sum();
    }
}