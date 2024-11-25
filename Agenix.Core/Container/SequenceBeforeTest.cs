using System.Linq;
using log4net;

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
    private static readonly ILog Log = LogManager.GetLogger(typeof(SequenceAfterTest));

    public override void DoExecute(TestContext context)
    {
        if (actions == null || actions.Count == 0) return;

        Log.Info("Entering before test block");

        if (Log.IsDebugEnabled)
        {
            Log.Debug("Executing " + actions.Count + " actions before test");
            Log.Debug("");
        }

        foreach (var action in actions.Select(actionBuilder => actionBuilder.Build())) action.Execute(context);
    }

    /// <summary>
    ///     Builder class for constructing instances of SequenceBeforeTest.
    /// </summary>
    public class Builder : AbstractTestBoundaryContainerBuilder<SequenceBeforeTest, Builder>
    {
        /**
         * Fluent API action building entry method used in C# DSL.
         * @return
         */
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