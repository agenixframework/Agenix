namespace Agenix.Screenplay.Questions;

/// <summary>
/// Represents a question that retrieves the maximum element from a collection of items answered by an actor.
/// </summary>
/// <typeparam name="T">
/// The type of the elements in the collection. It must implement <see cref="IComparable{T}"/>.
/// </typeparam>
public class MaxQuestion<T>(IQuestion<ICollection<T>> listQuestion, IComparer<T>? comparer)
    : IQuestion<T>
    where T : IComparable<T>
{
    public MaxQuestion(IQuestion<ICollection<T>> listQuestion)
        : this(listQuestion, Comparer<T>.Default)
    {
    }

    public T AnsweredBy(Actor actor)
    {
        var collection = listQuestion.AnsweredBy(actor);
        return comparer != null 
            ? collection.Max(comparer)
            : collection.Max();
    }
}
