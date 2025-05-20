using System;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Report;
using Agenix.Core.Report;

namespace Agenix.Core;

/// <summary>
///     Represents the core class within the Agenix framework, responsible for managing and providing access to the Agenix
///     context.
/// </summary>
public sealed class Agenix : ITestListenerAware, ITestSuiteListenerAware, ITestReporterAware, IMessageListenerAware
{
    /**
     * Constructor with given context that holds all basic Agenix components needed to run an Agenix project.
     * @param agenixContext
     */
    public Agenix(AgenixContext agenixContext)
    {
        AgenixContext = agenixContext;
    }

    /// <summary>
    ///     Provides access to the Agenix context, encapsulating various listeners, factories, registries, and configurations
    ///     used
    ///     for managing and executing tests within the Agenix framework. The AgenixContext is central to the setup and
    ///     lifecycle of
    ///     test execution, allowing customization and extension through various hooks and components.
    /// </summary>
    public AgenixContext AgenixContext { get; }

    /// Adds a message listener to the current Agenix context, allowing it to respond to message events during processing.
    /// <param name="listener">The message listener to be added.</param>
    public void AddMessageListener(IMessageListener listener)
    {
        AgenixContext.AddMessageListener(listener);
    }

    /// Adds a test listener to the Agenix context to track and respond to test events.
    /// @param testListener The ITestListener instance that will be notified of test events.
    /// /
    public void AddTestListener(ITestListener testListener)
    {
        AgenixContext.AddTestListener(testListener);
    }

    /// <summary>
    ///     Adds a test reporter to the Agenix context, allowing it to handle and generate reports for test results.
    /// </summary>
    /// <param name="testReporter">The test reporter to be added.</param>
    public void AddTestReporter(ITestReporter testReporter)
    {
        AgenixContext.AddTestReporter(testReporter);
    }

    /// <summary>
    ///     Registers a test suite listener to receive notifications of events regarding a test suite lifecycle.
    /// </summary>
    /// <param name="suiteListener">
    ///     The listener that will be added to receive test suite lifecycle events.
    /// </param>
    public void AddTestSuiteListener(ITestSuiteListener suiteListener)
    {
        AgenixContext.AddTestSuiteListener(suiteListener);
    }

    /// Creates a new instance of the Agenix class by invoking the instance manager's NewInstance method.
    /// @return A new instance of the Agenix class.
    /// /
    public static Agenix NewInstance()
    {
        return AgenixInstanceManager.NewInstance();
    }

    /// Creates a new Agenix instance using the provided context provider.
    /// <param name="contextProvider">The context provider to be used for creating the Agenix instance.</param>
    /// <return>The newly created Agenix instance.</return>
    /// /
    public static Agenix NewInstance(IAgenixContextProvider contextProvider)
    {
        return AgenixInstanceManager.NewInstance(contextProvider);
    }

    /// <summary>
    ///     Executes the required actions before a test suite begins running, validating applicable conditions.
    /// </summary>
    /// <param name="suiteName">The name of the test suite that is about to start.</param>
    /// <param name="testGroups">An optional list of test groups to be considered for execution before the suite begins.</param>
    /// <exception cref="Exception">Thrown if any of the before suite actions fail with errors.</exception>
    public void BeforeSuite(string suiteName, params string[] testGroups)
    {
        AgenixContext.SuiteListeners.OnStart();

        foreach (var sequenceBeforeSuite in AgenixContext.BeforeSuites)
            try
            {
                if (sequenceBeforeSuite.ShouldExecute(suiteName, testGroups))
                    sequenceBeforeSuite.Execute(AgenixContext.CreateTestContext());
            }
            catch (Exception e)
            {
                AgenixContext.SuiteListeners.OnStartFailure(e);
                AfterSuite(suiteName, testGroups);

                throw new Exception("Before suite failed with errors", e);
            }

        AgenixContext.SuiteListeners.OnStartSuccess();
    }

    /// <summary>
    ///     Finalizes the test suite execution by executing all registered
    ///     after-suite sequences and notifying the suite listeners about the completion.
    /// </summary>
    /// <param name="suiteName">The name of the test suite.</param>
    /// <param name="testGroups">The test groups associated with the test suite.</param>
    public void AfterSuite(string suiteName, params string[] testGroups)
    {
        AgenixContext.SuiteListeners.OnFinish();

        foreach (var sequenceAfterSuite in AgenixContext.AfterSuites)
            try
            {
                if (sequenceAfterSuite.ShouldExecute(suiteName, testGroups))
                    sequenceAfterSuite.Execute(AgenixContext.CreateTestContext());
            }
            catch (Exception e)
            {
                AgenixContext.SuiteListeners.OnFinishFailure(e);
                throw new Exception("After suite failed with errors", e);
            }

        AgenixContext.SuiteListeners.OnFinishSuccess();
    }

    /// Runs a test action, which can also be an entire test case, using the default test context.
    /// @param action The test action to be executed.
    /// /
    public void Run(ITestAction action)
    {
        Run(action, AgenixContext.CreateTestContext());
    }

    /// <summary>
    ///     Executes a test action within the specified test context.
    /// </summary>
    /// <param name="action">The test action to be executed, which can represent a single test or a complete test case.</param>
    /// <param name="testContext">
    ///     The context in which the test action will operate, providing necessary dependencies and
    ///     utilities.
    /// </param>
    public void Run(ITestAction action, TestContext testContext)
    {
        action.Execute(testContext);
    }

    /// <summary>
    ///     Closing Agenix and its context.
    /// </summary>
    public void Close()
    {
        AgenixContext.Close();
    }

    /// Retrieves the version of the Agenix application.
    /// @return The version string of the Agenix application.
    /// /
    public static string GetVersion()
    {
        return AgenixVersion.Version;
    }
}