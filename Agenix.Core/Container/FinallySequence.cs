namespace Agenix.Core.Container;

public class FinallySequence : Sequence
{
    /**
     * Default constructor.
     * 
     * @param builder
     */
    public FinallySequence(Builder builder) : base(
        (Sequence.Builder)new Sequence.Builder()
            .Description(builder.GetDescription())
            .Name(builder.GetName())
            .Actions(builder.GetActions().ToArray())
    )
    {
    }

    /// Builder class for constructing FinallySequence instances.
    /// /
    public new class Builder : AbstractTestContainerBuilder<FinallySequence, dynamic>
    {
        /**
         * Fluent API action building entry method used in C# DSL.
         * @return
         */
        public static Builder DoFinally()
        {
            return new Builder();
        }

        protected override FinallySequence DoBuild()
        {
            return new FinallySequence(this);
        }
    }
}