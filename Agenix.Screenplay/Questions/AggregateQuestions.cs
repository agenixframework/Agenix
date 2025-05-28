namespace Agenix.Screenplay.Questions;

/// <summary>
///     Provides a set of static methods for aggregating and transforming data from questions that return collections or
///     lists of values.
/// </summary>
public static class AggregateQuestions
{
    /// <summary>
    ///     Returns a question that computes the total number of elements
    ///     from a given question representing a collection.
    /// </summary>
    /// <param name="listQuestion">The question that provides a collection of items to be counted.</param>
    /// <returns>A question that, when answered, provides the total number of elements in the collection.</returns>
    public static IQuestion<int> TheTotalNumberOf<T>(IQuestion<ICollection<T>> listQuestion)
    {
        return new CountQuestion<T>(listQuestion);
    }

    /// <summary>
    ///     Returns a question that computes the sum of all integers
    ///     from a given question representing a collection of integers.
    /// </summary>
    /// <param name="listQuestion">The question that provides a collection of integers to be summed.</param>
    /// <returns>A question that, when answered, provides the sum of the integers in the collection.</returns>
    public static IQuestion<int> TheSumOf(IQuestion<ICollection<int>> listQuestion)
    {
        return new SumQuestion(listQuestion);
    }

    /// <summary>
    ///     Returns a question that computes the maximum value from a given question
    ///     representing a collection of comparable items.
    /// </summary>
    /// <param name="listQuestion">The question that provides a collection of items to determine the maximum value from.</param>
    /// <returns>A question that, when answered, provides the maximum value found in the collection.</returns>
    public static IQuestion<T> TheMaximumOf<T>(IQuestion<ICollection<T>> listQuestion) where T : IComparable<T>
    {
        return new MaxQuestion<T>(listQuestion);
    }

    /// <summary>
    ///     Retrieves the maximum value from a collection of items provided by the specified question.
    /// </summary>
    /// <param name="listQuestion">
    ///     The question that provides the collection of items from which the maximum will be
    ///     determined.
    /// </param>
    /// <typeparam name="T">The type of elements in the collection, which must implement <see cref="IComparable{T}" />.</typeparam>
    /// <returns>A question that, when answered, provides the maximum value in the collection.</returns>
    public static IQuestion<T> TheMaximumOf<T>(IQuestion<ICollection<T>> listQuestion,
        IComparer<T> comparer) where T : IComparable<T>
    {
        return new MaxQuestion<T>(listQuestion, comparer);
    }

    /// <summary>
    ///     Returns a question that computes the minimum value from a given question representing a collection of comparable
    ///     elements.
    /// </summary>
    /// <param name="listQuestion">The question that provides a collection of items to evaluate for the minimum value.</param>
    /// <returns>A question that, when answered, provides the minimum value of the elements in the collection.</returns>
    public static IQuestion<T> TheMinimumOf<T>(IQuestion<ICollection<T>> listQuestion) where T : IComparable<T>
    {
        return new MinQuestion<T>(listQuestion);
    }

    /// <summary>
    ///     Returns a question that computes the minimum value from a given question representing a collection of items.
    /// </summary>
    /// <param name="listQuestion">The question that provides a collection of items to find the minimum value from.</param>
    /// <param name="comparer">The comparer used to determine the minimum value.</param>
    /// <returns>A question that, when answered, provides the minimum value in the collection.</returns>
    public static IQuestion<T> TheMinimumOf<T>(IQuestion<ICollection<T>> listQuestion,
        IComparer<T> comparer) where T : IComparable<T>
    {
        return new MinQuestion<T>(listQuestion, comparer);
    }

    /// <summary>
    ///     Returns a question that computes the reversed order of elements
    ///     from a given question representing a list of items.
    /// </summary>
    /// <param name="listQuestion">The question that provides a list of items to be reversed.</param>
    /// <returns>A question that, when answered, provides the reversed list of elements.</returns>
    public static IQuestion<IList<T>> TheReverse<T>(IQuestion<IList<T>> listQuestion)
    {
        return new ReverseQuestion<T>(listQuestion);
    }

    /// <summary>
    ///     Returns a question that sorts the elements of a given list question
    ///     in ascending order using the default comparer for the element type.
    /// </summary>
    /// <param name="listQuestion">The question that provides a list of items to be sorted.</param>
    /// <returns>A question that, when answered, provides a sorted list of elements.</returns>
    public static IQuestion<IList<T>> TheSorted<T>(IQuestion<IList<T>> listQuestion) where T : IComparable<T>
    {
        return new SortedQuestion<T>(listQuestion);
    }

    /// <summary>
    ///     Returns a question that provides the sorted version of a given list of items as determined by the specified
    ///     comparer.
    /// </summary>
    /// <param name="listQuestion">The question that provides a list of items to be sorted.</param>
    /// <param name="comparer">The comparer to determine the order of the elements.</param>
    /// <returns>A question that, when answered, provides the sorted list of items.</returns>
    public static IQuestion<IList<T>> TheSorted<T>(IQuestion<IList<T>> listQuestion,
        IComparer<T> comparer) where T : IComparable<T>
    {
        return new SortedQuestion<T>(listQuestion, comparer);
    }
}