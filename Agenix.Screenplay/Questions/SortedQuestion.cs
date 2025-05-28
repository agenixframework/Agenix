namespace Agenix.Screenplay.Questions;

/// <summary>
/// A question that returns a sorted list of comparable items.
/// </summary>
/// <typeparam name="T">The type of items to sort, must be comparable</typeparam>
public class SortedQuestion<T>(IQuestion<IList<T>> listQuestion, IComparer<T> comparer) : IQuestion<IList<T>>
    where T : IComparable<T>
{
    public SortedQuestion(IQuestion<IList<T>> listQuestion) 
        : this(listQuestion, Comparer<T>.Default)
    {
    }

    public IList<T> AnsweredBy(Actor actor)
    {
        var sortedItems = new List<T>(listQuestion.AnsweredBy(actor));
        sortedItems.Sort(comparer);
        return sortedItems;
    }
}