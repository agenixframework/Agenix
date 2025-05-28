namespace Agenix.Screenplay.Exceptions;

/// <summary>
/// Represents an exception that is thrown when no ability
/// matching the expected criteria is found within the context
/// of the screenplay scenario.
/// </summary>
/// <remarks>
/// This exception is typically used in scenarios where an
/// inability to find a required ability disrupts the expected
/// flow of the program.
/// </remarks>
/// <param name="message">
/// The message that describes the error.
/// </param>
public class NoMatchingAbilityException(string message) : SystemException(message);