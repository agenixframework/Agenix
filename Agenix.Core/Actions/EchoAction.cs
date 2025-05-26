using System;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Actions;

/// <summary>
///     Represents a test action that logs messages to the console or configured logger
///     during the execution of automation tests.
/// </summary>
public class EchoAction : AbstractTestAction
{
    /// Logger for EchoAction.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(EchoAction));

    /// Represents the log message associated with an EchoAction instance.
    /// This message is used to log information during the execution of the test action.
    private readonly string Message;

    /// Represents a test action that logs messages to the console or configured logger
    /// during the execution of automation tests.
    /// /
    private EchoAction(Builder builder) : base("echo", builder)
    {
        Message = builder._message;
    }

    public override void DoExecute(TestContext context)
    {
        Log.LogInformation(Message == null
            ? $"Agenix test {DateTime.Now}"
            : context.LogModifier.Mask(context.ReplaceDynamicContentInString(Message)));
    }

    /// Retrieves the log message for the action.
    /// @return the message as a string
    /// /
    public string GetMessage()
    {
        return Message;
    }

    /// <summary>
    ///     Builder class for creating and configuring EchoAction instances.
    /// </summary>
    public sealed class Builder : AbstractTestActionBuilder<ITestAction, dynamic>
    {
        public string _message;

        /// Fluent API action building entry method used in C# DSL.
        /// @return Builder instance for creating an Echo action.
        /// /
        public static Builder Echo()
        {
            return new Builder();
        }

        /// Fluent API action building entry method used in C# DSL.
        /// @param message The message content to be used for the Echo action.
        /// @return A builder instance configured with the specified message.
        /// /
        public static Builder Echo(string message)
        {
            Builder builder = new();
            builder.Message(message);
            return builder;
        }

        /// Fluent API action building entry method used in C# DSL.
        /// @return A builder instance for creating an action with default configuration.
        /// /
        public static Builder Print()
        {
            return new Builder();
        }

        /// Fluent API action building entry method used in C# DSL.
        /// Creates a builder instance for defining an Echo action with the specified message content.
        /// <param name="message">The message to be printed by the Echo action.</param>
        /// <return>Returns a builder instance configured with the specified message.</return>
        /// /
        public static Builder Print(string message)
        {
            Builder builder = new();
            builder.Message(message);
            return builder;
        }

        /// Represents a message action used within the Agenix framework.
        /// The EchoAction allows configuration and execution of actions involving message handling.
        /// /
        public Builder Message(string message)
        {
            _message = message;
            return this;
        }

        /// <summary>
        /// Sets the name of the action and returns the builder instance for further configuration.
        /// </summary>
        /// <param name="name">The name to be assigned to the action.</param>
        /// <returns>The updated builder instance.</returns>
        public override Builder Name(string name)
        {
            base.Name(name);
            return this;
        }

        /// <summary>
        /// Sets the description for the test action being built.
        /// </summary>
        /// <param name="description">The description to assign to the test action.</param>
        /// <returns>The builder instance for chaining further configurations.</returns>
        public override Builder Description(string description)
        {
            base.Description(description);
            return this;
        }

        public override EchoAction Build()
        {
            return new EchoAction(this);
        }
    }
}