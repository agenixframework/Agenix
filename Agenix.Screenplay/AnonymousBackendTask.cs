using Agenix.Screenplay.Annotations;

namespace Agenix.Screenplay;

/// <summary>
///     Represents an anonymous backend task that can be performed by an actor.
/// </summary>
/// <remarks>
///     The <c>AnonymousBackendTask</c> class extends <see cref="AnonymousTask" />
///     and inherits its properties and functionality. It is intended to define backend-specific
///     tasks that follow the screenplay pattern.
/// </remarks>
/// <seealso cref="AnonymousTask" />
/// <seealso cref="IPerformable" />
/// <seealso cref="ITask" />
public class AnonymousBackendTask : AnonymousTask
{
    public AnonymousBackendTask()
    {
    }

    public AnonymousBackendTask(string title, List<IPerformable> steps)
        : base(title, steps)
    {
    }
}
