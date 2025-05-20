using System.Linq;
using Agenix.Api.Container;
using Agenix.Api.Context;
using log4net;

namespace Agenix.Core.Container;

/// <summary>
///     Sequence of test actions executed after a test case. Container execution can be restricted according to test name ,
///     namespace and test groups.
/// </summary>
public class SequenceAfterTest : AbstractTestBoundaryActionContainer, IAfterTest
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(SequenceAfterTest));

    /// Executes the set of actions contained in the SequenceAfterTest after a test is completed.
    /// <param name="context">The context in which the actions are executed.</param>
    public override void DoExecute(TestContext context)
    {
        if (actions is { Count: 0 }) return;

        Log.Info("Entering after test block");

        if (Log.IsDebugEnabled)
        {
            Log.Debug("Executing " + actions.Count + " actions after test");
            Log.Debug("");
        }

        foreach (var action in actions.Select(actionBuilder => actionBuilder.Build())) action.Execute(context);
    }

    /// The Builder class is a concrete implementation of the AbstractTestBoundaryContainerBuilder used for constructing instances
    /// of SequenceAfterTest with specific configurations. This class provides a fluent API for setting various testing conditions.
    public class Builder : AbstractTestBoundaryContainerBuilder<SequenceAfterTest, Builder>
    {
        /// Fluent API action building entry method used in C# DSL.
        /// @return
        /// /
        public static Builder AfterTest()
        {
            return new Builder();
        }

        /// Builds and returns an instance of SequenceAfterTest.
        /// @return An instance of SequenceAfterTest.
        protected override SequenceAfterTest DoBuild()
        {
            return new SequenceAfterTest();
        }
    }
}