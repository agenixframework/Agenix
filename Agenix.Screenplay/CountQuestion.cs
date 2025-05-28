using System.Collections;

namespace Agenix.Screenplay;

/// <summary>
/// Represents a question that retrieves the count of elements in a collection.
/// </summary>
public class CountQuestion<T>(IQuestion<ICollection<T>> listQuestion) : IQuestion<int>
{
    public int AnsweredBy(Actor actor)
    {
        return listQuestion.AnsweredBy(actor).Count;
    }
}
