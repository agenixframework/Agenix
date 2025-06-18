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

namespace Agenix.Core.Container;

/// Represents a sequence of actions that will execute a set of operations
/// and ensures that a final operation is performed at the end.
public class FinallySequence : Sequence
{
    /// Represents a sequence of actions that will execute a set of operations
    /// and ensures that a final operation is performed at the end.
    /// /
    public FinallySequence(Builder builder) : base(
        new Sequence.Builder()
            .Description(builder.GetDescription())
            .Name(builder.GetName())
            .Actions(builder.GetActions().ToArray())
    )
    {
    }

    /// Builder class for constructing FinallySequence instances.
    /// /
    public new class Builder : AbstractTestContainerBuilder<FinallySequence, dynamic>, ITestAction
    {
        public void Execute(TestContext context)
        {
            DoBuild().Execute(context);
        }

        /// Fluent API action building entry method used in C# DSL.
        /// @return A new instance of the Builder class for constructing FinallySequence instances.
        /// /
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
