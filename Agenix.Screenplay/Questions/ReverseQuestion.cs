namespace Agenix.Screenplay.Questions;

/// <summary>
/// A question that returns a reversed list of items.
/// </summary>
/// <typeparam name="T">The type of items in the list</typeparam>
public class ReverseQuestion<T>(IQuestion<IList<T>> listQuestion) : IQuestion<IList<T>>
{
    public IList<T> AnsweredBy(Actor actor)
    {
        var list = new List<T>(listQuestion.AnsweredBy(actor));
        list.Reverse();
        return list;
    }
}
