namespace Agenix.Screenplay;

/// <summary>
/// Represents a consequence that can be evaluated within the context of the Screenplay pattern.
/// </summary>
/// <typeparam name="T">The type of value associated with the consequence.</typeparam>
public interface IConsequence<T>
{
    void EvaluateFor(Actor actor);
    IConsequence<T> OrComplainWith(Type complaintType);
    IConsequence<T> OrComplainWith(Type complaintType, string complaintDetails);
    IConsequence<T> WhenAttemptingTo(IPerformable performable);
    IConsequence<T> Because(string explanation);

    /// <summary>
    /// Evaluate the consequence only after performing the specified tasks.
    /// </summary>
    IConsequence<T> After(params IPerformable[] setupActions);
}
