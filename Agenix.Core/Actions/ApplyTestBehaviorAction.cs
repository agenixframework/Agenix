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

using Agenix.Api;
using Agenix.Api.Context;

namespace Agenix.Core.Actions;

/// <summary>
///     Represents an action that applies a test behavior using a specified runner.
/// </summary>
public class ApplyTestBehaviorAction(ApplyTestBehaviorAction.Builder builder)
    : AbstractTestAction(builder.GetName() ?? "apply-behaviour", builder.GetDescription())
{
    private readonly ITestBehavior _behavior = builder._behavior;
    private readonly ITestActionRunner _runner = builder._runner;

    public override void DoExecute(TestContext context)
    {
        _behavior.Apply(_runner);
    }

    /// <summary>
    ///     A builder class for creating instances of ApplyTestBehaviorAction.
    /// </summary>
    public class Builder : AbstractTestActionBuilder<ApplyTestBehaviorAction, Builder>,
        ITestActionBuilder<ApplyTestBehaviorAction>
    {
        internal ITestBehavior _behavior;
        internal ITestActionRunner _runner;

        /// <summary>
        ///     Constructs an instance of <see cref="ApplyTestBehaviorAction" /> using the current state of the builder.
        /// </summary>
        /// <returns>An instance of <see cref="ApplyTestBehaviorAction" />.</returns>
        public override ApplyTestBehaviorAction Build()
        {
            return new ApplyTestBehaviorAction(this);
        }

        /// <summary>
        ///     Creates a new instance of the Builder.
        /// </summary>
        /// <returns>A new instance of the Builder.</returns>
        public static Builder Apply()
        {
            return new Builder();
        }

        /// <summary>
        ///     Creates a new instance of the Builder with the provided behavior.
        /// </summary>
        /// <param name="behavior">The behavior to be applied to the builder.</param>
        /// <returns>A new instance of the Builder with the behavior set.</returns>
        public static Builder Apply(ITestBehavior behavior)
        {
            var builder = new Builder { _behavior = behavior };
            return builder;
        }

        /// <summary>
        ///     Creates a new instance of the Builder with the specified test behavior.
        /// </summary>
        /// <param name="behavior">The specific test behavior to be applied to the builder.</param>
        /// <returns>A new instance of the Builder with the specified test behavior set.</returns>
        public static Builder Apply(TestBehavior behavior)
        {
            return Apply(new DelegatingTestBehaviour(behavior));
        }

        /// <summary>
        ///     Sets the specified behavior for the builder.
        /// </summary>
        /// <param name="behavior">The behavior to be applied to the builder.</param>
        /// <returns>The builder instance with the behavior set.</returns>
        public Builder Behavior(ITestBehavior behavior)
        {
            _behavior = behavior;
            return this;
        }

        /// <summary>
        ///     Sets the specified behavior for the builder using the TestBehavior delegate.
        /// </summary>
        /// <param name="behavior">The TestBehavior delegate to be applied to the builder.</param>
        /// <returns>The builder instance with the behavior set through a DelegatingTestBehaviour.</returns>
        public Builder Behavior(TestBehavior behavior)
        {
            return Behavior(new DelegatingTestBehaviour(behavior));
        }

        /// <summary>
        ///     Assigns the provided test action runner to the builder.
        /// </summary>
        /// <param name="runner">The test action runner to be assigned.</param>
        /// <returns>The builder instance with the runner assigned.</returns>
        public Builder On(ITestActionRunner runner)
        {
            _runner = runner;
            return this;
        }
    }
}
