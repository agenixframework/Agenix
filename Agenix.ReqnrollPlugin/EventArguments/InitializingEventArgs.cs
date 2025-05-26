namespace Agenix.ReqnrollPlugin.EventArguments;

/// <summary>
///     Represents event data for the initializing event in the Agenix Reqnroll plugin.
/// </summary>
public class InitializingEventArgs(Core.Agenix agenix)
{
    /// <summary>
    ///     Represents the core class of the Agenix system, acting as the central entry point for managing and interacting with
    ///     testing-related functionalities.
    /// </summary>
    /// <remarks>
    ///     The `Agenix` class is responsible for coordinating test execution, suite initialization, reporting mechanisms,
    ///     and listener integrations within the Agenix framework. It serves as a foundational component, encapsulating
    ///     configuration management, context provisioning, and test action execution.
    /// </remarks>
    /// <remarks>
    ///     This class supports adding and managing test-related listeners, reporters, and suites, ensuring a flexible and
    ///     extensible workflow for test execution processes. It also provides utility methods for versioning, resource
    ///     management, and lifecycle operations.
    /// </remarks>
    public Core.Agenix Agenix { get; set; } = agenix;
}