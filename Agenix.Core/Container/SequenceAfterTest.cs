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

using System.Linq;
using Agenix.Api.Container;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

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
    private static readonly ILogger Log = LogManager.GetLogger(typeof(SequenceAfterTest));

    /// Executes the set of actions contained in the SequenceAfterTest after a test is completed.
    /// <param name="context">The context in which the actions are executed.</param>
    public override void DoExecute(TestContext context)
    {
        if (actions is { Count: 0 }) return;

        Log.LogInformation("Entering after test block");

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Executing " + actions.Count + " actions after test");
            Log.LogDebug("");
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
