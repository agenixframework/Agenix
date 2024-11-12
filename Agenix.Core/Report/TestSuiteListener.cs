using System;

namespace Agenix.Core.Report;

/// Listener for events regarding a test suite lifecycle.
/// /
public interface ITestSuiteListener
{
    /// Invoked on test suite start.
    /// /
    void OnStart();

    /// Invoked after successful test suite start.
    void OnStartSuccess();

    /// Invoked after failed test suite start.
    /// <param name="cause">The exception cause of the failure.</param>
    void OnStartFailure(Exception cause);

    /// Invoked on test suite finish.
    void OnFinish();

    /// Invoked after successful test suite finish.
    void OnFinishSuccess();

    /// Invoked after failed test suite finish.
    /// <param name="cause">The exception cause of the failure.</param>
    void OnFinishFailure(Exception cause);
}