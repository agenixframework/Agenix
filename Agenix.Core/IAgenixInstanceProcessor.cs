namespace Agenix.Core;

/// <summary>
///     Represents a delegate for processing an Agenix instance during its creation process.
/// </summary>
/// <param name="instance">The Agenix instance to be processed.</param>
public delegate void AgenixInstanceProcessorDelegate(Agenix instance);

/// <summary>
///     Agenix instance processor takes part in instance creation process.
/// </summary>
public interface IAgenixInstanceProcessor
{
    /// <summary>
    ///     Process Agenix instance after this has been instantiated.
    /// </summary>
    /// <param name="instance">The Agenix instance to process.</param>
    void Process(Agenix instance);
}