using Agenix.Screenplay.Questions;

namespace Agenix.Screenplay;

/// <summary>
///     Represents a function that creates a question answer based on an actor.
/// </summary>
/// <typeparam name="T">The type of answer returned by the question.</typeparam>
/// <param name="actor">The actor who will answer the question.</param>
/// <returns>The answer to the question.</returns>
public delegate T QuestionAnswerProvider<T>(Actor actor);

/// <summary>
///     Builder class for constructing questions with a specific subject in a Screenplay pattern.
/// </summary>
public class QuestionBuilder(string subject)
{
    /// <summary>
    ///     Associates a subject to a question and returns a new question instance with the defined subject.
    /// </summary>
    /// <typeparam name="T">The type of the answer returned by the question.</typeparam>
    /// <param name="questionToAsk">The question to be associated with the subject.</param>
    /// <returns>A new question instance with the defined subject.</returns>
    public IQuestion<T> AnsweredBy<T>(IQuestion<T> questionToAsk)
    {
        return new QuestionWithDefinedSubject<T>(questionToAsk, subject);
    }

    /// <summary>
    ///     Creates a question from a delegate function that provides the answer.
    /// </summary>
    /// <typeparam name="T">The type of the answer returned by the question.</typeparam>
    /// <param name="answerProvider">A delegate function that provides the answer when given an actor.</param>
    /// <returns>A new question instance with the defined subject.</returns>
    public IQuestion<T> AnsweredBy<T>(QuestionAnswerProvider<T> answerProvider)
    {
        return new DelegateQuestion<T>(answerProvider, subject);
    }

    /// <summary>
    ///     Represents a question implemented using a delegate function.
    /// </summary>
    /// <typeparam name="T">The type of the answer provided by this question.</typeparam>
    private class DelegateQuestion<T> : IQuestion<T>
    {
        private readonly QuestionAnswerProvider<T> _answerProvider;

        /// <summary>
        ///     Initializes a new instance of the DelegateQuestion class.
        /// </summary>
        /// <param name="answerProvider">The delegate function that provides the answer.</param>
        /// <param name="subject">The subject of the question.</param>
        public DelegateQuestion(QuestionAnswerProvider<T> answerProvider, string subject)
        {
            _answerProvider = answerProvider ?? throw new ArgumentNullException(nameof(answerProvider));
            Subject = subject ?? string.Empty;
        }

        /// <summary>
        ///     Gets the answer to the question for the given actor.
        /// </summary>
        /// <param name="actor">The actor who will answer the question.</param>
        /// <returns>The answer to the question.</returns>
        public T AnsweredBy(Actor actor)
        {
            return _answerProvider(actor);
        }

        /// <summary>
        ///     Gets the subject of the question.
        /// </summary>
        public string Subject { get; }
    }
}