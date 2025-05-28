namespace Agenix.Screenplay.Questions;

/// <summary>
///     Represents a question that includes a specifically defined subject.
/// </summary>
/// <typeparam name="T">The type of the answer provided by this question.</typeparam>
public class QuestionWithDefinedSubject<T>(IQuestion<T> theQuestion, string subject, List<IPerformable> precedingTasks)
    : IQuestion<T>
{
    public QuestionWithDefinedSubject(IQuestion<T> theQuestion, string subject)
        : this(theQuestion, subject, [])
    {
    }

    /// <summary>
    ///     Provides the answer to the question for the given actor after executing any preceding tasks.
    /// </summary>
    /// <param name="actor">The actor who will answer the question and execute any preceding tasks.</param>
    /// <returns>The answer to the question provided by the given actor.</returns>
    public T AnsweredBy(Actor actor)
    {
        actor.AttemptsTo(Actor.ErrorHandlingMode.IGNORE_EXCEPTIONS, precedingTasks.ToArray());
        return theQuestion.AnsweredBy(actor);
    }

    /// <summary>
    ///     Represents the subject or title of the question being asked.
    /// </summary>
    /// <remarks>
    ///     The property provides a description or label that defines the focus of the question.
    ///     It is typically used to make the question more understandable or specific when logging, debugging,
    ///     or presenting it to users.
    /// </remarks>
    public string Subject { get; } = subject;
}