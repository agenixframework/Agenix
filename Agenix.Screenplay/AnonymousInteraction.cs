namespace Agenix.Screenplay;

/// <summary>
/// Represents an interaction that can be defined anonymously with a specific title and a list of steps.
/// Extends the functionality of AnonymousPerformable and implements the Interaction interface.
/// </summary>
public class AnonymousInteraction(string title, List<IPerformable> steps)
    : AnonymousPerformable(title, steps), Interaction
{
    public AnonymousPerformableFieldSetter<AnonymousInteraction> With(string fieldName)
    {
        return new AnonymousPerformableFieldSetter<AnonymousInteraction>(this, fieldName);
    }
}
