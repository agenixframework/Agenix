using System;
using Agenix.Api;
using Agenix.Api.Context;
using log4net;

namespace Agenix.Core.Actions;

/**
* Prints messages to the console/logger during test execution.
*/
public class EchoAction : AbstractTestAction
{
    /// Logger for EchoAction.
    /// /
    private static readonly ILog Log = LogManager.GetLogger(typeof(EchoAction));

    /**
     * Log message
     */
    private readonly string Message;

    /**
     * Default constructor using the builder.
     * @param builder
     */
    private EchoAction(Builder builder) : base("echo", builder)
    {
        Message = builder._message;
    }

    public override void DoExecute(TestContext context)
    {
        Log.Info(Message == null
            ? $"Agenix test {DateTime.Now}"
            : context.LogModifier.Mask(context.ReplaceDynamicContentInString(Message)));
    }

    /**
     * Gets the message.
     * @return the message
     */
    public string GetMessage()
    {
        return Message;
    }

    /**
     * Action builder.
     */
    public sealed class Builder : AbstractTestActionBuilder<ITestAction, dynamic>
    {
        public string _message;

        /**
         * Fluent API action building entry method used in C# DSL.
         * @return
         */
        public static Builder Echo()
        {
            return new Builder();
        }

        /**
         * Fluent API action building entry method used in C# DSL.
         * @param message
         * @return
         */
        public static Builder Echo(string message)
        {
            Builder builder = new();
            builder.Message(message);
            return builder;
        }

        /**
         * Fluent API action building entry method used in C# DSL.
         * @return
         */
        public static Builder Print()
        {
            return new Builder();
        }

        /**
         * Fluent API action building entry method used in C# DSL.
         * @param message
         * @return
         */
        public static Builder Print(string message)
        {
            Builder builder = new();
            builder.Message(message);
            return builder;
        }

        public Builder Message(string message)
        {
            _message = message;
            return this;
        }

        public override EchoAction Build()
        {
            return new EchoAction(this);
        }
    }
}