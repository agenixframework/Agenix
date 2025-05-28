namespace Agenix.Screenplay;

/// <summary>
///     Represents an interface for entities capable of performing tasks and asking questions.
/// </summary>
public interface IPerformsTasks
{
    /// <summary>
    ///     Allows an actor to perform one or more tasks or actions represented by the <see cref="IPerformable" /> interface.
    /// </summary>
    /// <typeparam name="T">The type of tasks or actions to be performed, which must implement <see cref="IPerformable" />.</typeparam>
    /// <param name="todos">A collection of tasks or actions that the actor will attempt to perform.</param>
    void AttemptsTo<T>(params T[] todos) where T : IPerformable;

    /// <summary>
    ///     Allows an actor to request an answer to a specified question represented by the <see cref="IQuestion{TAnswer}" />
    ///     interface.
    /// </summary>
    /// <typeparam name="ANSWER">The type of the answer expected from the question.</typeparam>
    /// <param name="question">The question from which the actor seeks an answer.</param>
    /// <returns>The result of the question, which is of type <typeparamref name="ANSWER" />.</returns>
    ANSWER AsksFor<ANSWER>(IQuestion<ANSWER> question);
}