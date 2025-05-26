using System;
using Agenix.Api.Context;
using Agenix.Core;

namespace Agenix.ReqnrollPlugin.EventArguments;

/// <summary>
///     Provides data for the event triggered when a run starts.
/// </summary>
public class RunStartedEventArgs(AgenixContext agenixContext) : EventArgs
{
    /// <summary>
    ///     Represents the arguments for the event raised when a run starts.
    ///     Provides contextual information about the run initiation process.
    /// </summary>
    public RunStartedEventArgs(AgenixContext agenixContext, TestContext testContext) : this(agenixContext)
    {
        TestContext = testContext;
    }

    /// <summary>
    ///     Represents the core context implementation in Agenix, encapsulating primary components and services
    ///     used throughout the system.
    /// </summary>
    /// <remarks>
    ///     This type is integral to configuring, managing, and coordinating various aspects of the Agenix framework,
    ///     including test listeners, action listeners, reporting, validation, messaging, configuration, and more.
    ///     It implements multiple interfaces to facilitate extensibility by handling specific framework behaviors.
    /// </remarks>
    public AgenixContext AgenixContext { get; } = agenixContext;

    public TestContext TestContext { get; set; }

    public bool Canceled { get; set; }
}