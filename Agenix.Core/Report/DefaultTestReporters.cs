using System.Collections.Generic;
using Agenix.Api.Report;

namespace Agenix.Core.Report;

/// Provides a default set of test reporters to be used during test executions.
/// This class automatically adds a predefined list of test reporters upon instantiation, facilitating
/// the test reporting mechanism with minimal configuration.
/// It derives from the base class TestReporters, utilizing its methods to manage the addition
/// and orchestration of test reporters.
public class DefaultTestReporters : TestReporters
{
    public static readonly List<ITestReporter> DefaultReporters = [new LoggingReporter()];

    public DefaultTestReporters()
    {
        DefaultReporters.ForEach(AddTestReporter);
    }
}