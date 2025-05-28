namespace Agenix.Screenplay.Exceptions;

/// <summary>
/// Represents an exception that occurs when the instantiation of a task fails.
/// </summary>
/// <remarks>
/// This exception is typically thrown when a task's initialization
/// cannot be completed due to improper configuration, missing dependencies,
/// or other runtime errors that prevent task creation.
/// </remarks>
public class TaskInstantiationException(string message) : SystemException(message);