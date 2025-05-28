namespace Agenix.Screenplay.Questions;

/// <summary>
/// Represents a question that retrieves the minimum value from a collection of items.
/// </summary>
/// <typeparam name="T">The type of items in the collection. This type must implement the IComparable interface.</typeparam>
public class MinQuestion<T>(IQuestion<ICollection<T>> listQuestion, IComparer<T>? comparer)
    : IQuestion<T>
    where T : IComparable<T>
{
    public MinQuestion(IQuestion<ICollection<T>> listQuestion)
        : this(listQuestion, Comparer<T>.Default)
    {
    }

    public T AnsweredBy(Actor actor)
    {
        var collection = listQuestion.AnsweredBy(actor);
        return comparer != null 
            ? collection.Min(comparer)
            : collection.Min();
    }
}
