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
using Agenix.Core.Condition;

namespace Agenix.Core.Container;

/// Provides a builder for constructing a WaitActionCondition. Inherits from WaitConditionBuilder
/// and specializes the process for handling ActionConditions. This class is used to define
/// the action that will be tested and waited on, similar to other condition-based wait builders in the system.
public class WaitActionConditionBuilder : WaitConditionBuilder<ActionCondition, WaitActionConditionBuilder>
{
    /// Constructs an instance of WaitActionConditionBuilder using the specified builder.
    /// @param builder The builder for constructing the WaitActionConditionBuilder instance.
    /// /
    public WaitActionConditionBuilder(Wait.Builder<ActionCondition> builder) : base(builder)
    {
    }

    /// Sets the test action to execute and wait for.
    /// @param action The test action to be set.
    /// @return The updated WaitActionConditionBuilder with the specified test action set.
    /// /
    public WaitActionConditionBuilder Action(ITestAction action)
    {
        GetCondition().SetAction(action);
        return this;
    }

    /// Sets the test action using the specified action builder and returns the updated WaitActionConditionBuilder.
    /// @param action The test action builder that will provide the test action to be set.
    /// @return The updated WaitActionConditionBuilder with the specified test action set.
    /// /
    public WaitActionConditionBuilder Action(ITestActionBuilder<ITestAction> action)
    {
        GetCondition().SetAction(action.Build());
        return this;
    }
}
