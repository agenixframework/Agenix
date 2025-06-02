#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using Agenix.Api.Context;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Container;

/// Sequence container executing a set of nested test actions in a simple sequence.
/// /
public class Sequence(Sequence.Builder builder) : AbstractActionContainer(builder.GetName() ?? "sequential",
    builder.GetDescription(), builder.GetActions())
{
    /// <summary>
    ///     A logger instance for the Sequence class, used to record log messages
    ///     related to the execution of a sequence of test actions.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(Sequence));

    /// Executes the sequence of nested test actions in the provided context.
    /// @param context The context in which the test actions are executed.
    /// /
    public override void DoExecute(TestContext context)
    {
        foreach (var actionBuilder in actions)
        {
            ExecuteAction(actionBuilder.Build(), context);
        }

        Log.LogDebug("Action sequence finished successfully.");
    }

    /// Builder class for constructing instances of the Sequence action.
    public sealed class Builder : AbstractTestContainerBuilder<Sequence, Builder>
    {
        /// Initializes a builder for constructing a Sequence container.
        /// A Sequence container ensures actions are executed in a sequential order.
        /// <returns>A builder instance for creating a Sequence container.</returns>
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
