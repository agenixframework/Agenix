using System.Linq;
using Agenix.Api.Container;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Container;

/// <summary>
///     Sequence of test actions executed before a test case. Container execution can be restricted according to test
///     name,namespace and test groups.
/// </summary>
public class SequenceBeforeTest : AbstractTestBoundaryActionContainer, IBeforeTest
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(SequenceAfterTest));

    /// Executes a sequence of actions before a test is run.
    /// <param name="context">
    ///     The context in which the test actions are executed, providing state and environment information
    ///     for the actions.
    /// </param>
    public override void DoExecute(TestContext context)
    {
        if (actions == null || actions.Count == 0) return;

        Log.LogInformation("Entering before test block");

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Executing " + actions.Count + " actions before test");
            Log.LogDebug("");
        }

        foreach (var action in actions.Select(actionBuilder => actionBuilder.Build())) action.Execute(context);
    }

    /// <summary>
    ///     Builder class for constructing instances of SequenceBeforeTest.
    /// </summary>
    public class Builder : AbstractTestBoundaryContainerBuilder<SequenceBeforeTest, Builder>
    {
        /// Fluent API action building entry method used in C# DSL.
        /// @return A new instance of the Builder class for constructing SequenceBeforeTest instances.
        /// /
        public static Builder BeforeTest()
        {
            return new Builder();
        }

        protected override SequenceBeforeTest DoBuild()
        {
            return new SequenceBeforeTest();
        }
    }
}