namespace Agenix.Screenplay.Cast;

/// <summary>
///     Represents an exception that is thrown when there is no stage available in the context of the screenplay.
/// </summary>
/// <remarks>
///     This exception is intended to indicate that a required stage or context for the screenplay operation is missing.
/// </remarks>
/// <seealso cref="SystemException" />
public class NoStageException(string message) : SystemException(message);