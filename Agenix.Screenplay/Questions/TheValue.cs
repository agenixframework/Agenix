namespace Agenix.Screenplay.Questions;

/// <summary>
/// Convenience class to convert an object into a Question
/// </summary>
public static class TheValue
{
    /// <summary>
    /// Creates a question that represents the given value.
    /// </summary>
    /// <typeparam name="TAnswer">The type of the value that the question will produce.</typeparam>
    /// <param name="value">The value to represent as a question.</param>
    /// <returns>An IQuestion instance containing the provided value.</returns>
    public static IQuestion<TAnswer> Of<TAnswer>(TAnswer value)
    {
        return new SimpleQuestion<TAnswer>(actor => value);
    }

    /// <summary>
    /// Creates a question representing the given value.
    /// </summary>
    /// <typeparam name="TAnswer">The type of the value this question will produce.</typeparam>
    /// <param name="value">The value to represent as a question.</param>
    /// <returns>An IQuestion instance containing the supplied value.</returns>
    public static IQuestion<TAnswer> Of<TAnswer>(string subject, TAnswer value)
    {
        return new QuestionWithDefinedSubject<TAnswer>(Of(value), subject);
    }
}

internal class SimpleQuestion<TAnswer> : IQuestion<TAnswer>
{
    private readonly Func<Actor, TAnswer> _answerFunction;

    public SimpleQuestion(Func<Actor, TAnswer> answerFunction)
    {
        _answerFunction = answerFunction;
    }

    public TAnswer AnsweredBy(Actor actor)
    {
        return _answerFunction(actor);
    }
}
