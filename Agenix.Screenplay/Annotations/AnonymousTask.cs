namespace Agenix.Screenplay;

/// <summary>
/// Represents an anonymous task that can be performed by an actor.
/// </summary>
/// <remarks>
/// AnonymousTask extends the functionality of <see cref="AnonymousPerformable"/>
/// by implementing the <see cref="ITask"/> interface. This allows defining tasks
/// with a title, steps, and custom field values to be used within a screenplay-style
/// testing framework.
/// </remarks>
public class AnonymousTask : AnonymousPerformable, ITask
{
    
    public AnonymousTask()
    {
    }

    public AnonymousTask(string title, List<IPerformable> steps)
        : base(title, steps)
    {
    }

    /// <summary>
    /// Sets a custom field value for the current AnonymousTask instance.
    /// </summary>
    /// <param name="fieldName">The name of the field to be set.</param>
    /// <returns>An instance of <see cref="AnonymousPerformableFieldSetter{AnonymousTask}"/>
    /// that enables chaining to specify the field value.</returns>
    public AnonymousPerformableFieldSetter<AnonymousTask> With(string fieldName)
    {
        return new AnonymousPerformableFieldSetter<AnonymousTask>(this, fieldName);
    }
    
}
