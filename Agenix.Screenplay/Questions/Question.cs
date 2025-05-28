namespace Agenix.Screenplay.Questions;

/// <summary>
///     Represents a generic question that can be answered by an actor in the system.
/// </summary>
/// <typeparam name="TAnswer">The type of the answer that the question yields.</typeparam>
public class Question<TAnswer>(Func<Actor, TAnswer> answeredBy) : IQuestion<TAnswer>
{
    public TAnswer AnsweredBy(Actor actor)
    {
        return answeredBy(actor);
    }
}