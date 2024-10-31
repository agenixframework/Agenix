using log4net;

namespace Agenix.Core.Container;

/// Sequence container executing a set of nested test actions in a simple sequence.
/// /
public class Sequence(Sequence.Builder builder) : AbstractActionContainer("sequential", builder)
{
    private static readonly ILog _log = LogManager.GetLogger(typeof(Sequence));

    /// Executes the sequence of nested test actions in the provided context.
    /// @param context The context in which the test actions are executed.
    /// /
    public override void DoExecute(TestContext context)
    {
        foreach (var actionBuilder in actions) ExecuteAction(actionBuilder.Build(), context);

        _log.Debug("Action sequence finished successfully.");
    }

    /// Builder class for constructing instances of the Sequence action.
    public sealed class Builder : AbstractTestContainerBuilder<ITestActionContainer,
        ITestActionContainerBuilder<ITestActionContainer>>
    {
        /**
         * Fluent API action building entry method used in C# DSL.
         * @return
         */
        public static Builder Sequential()
        {
            return new Builder();
        }

        protected override Sequence DoBuild()
        {
            return new Sequence(this);
        }
    }
}