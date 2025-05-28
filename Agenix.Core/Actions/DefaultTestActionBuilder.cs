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

/// Builder for creating default test actions.
/// This class provides a concrete implementation for building test actions using a delegate TestAction.
public class DefaultTestActionBuilder(TestAction action)
    : AbstractTestActionBuilder<ITestAction, DefaultTestActionBuilder>
{
    /// Static fluent entry method for creating a DefaultTestActionBuilder.
    /// @param action The ITestAction instance used to create the builder.
    /// @return A new instance of DefaultTestActionBuilder.
    /// /
    public static DefaultTestActionBuilder Action(TestAction action)
    {
        return new DefaultTestActionBuilder(action);
    }

    /// Builds an instance of ConcreteTestAction using the provided delegate.
    /// Sets the name and description for the constructed test action.
    /// @return An instance of AbstractTestAction populated with the assigned Name and Description.
    public override AbstractTestAction Build()
    {
        AbstractTestAction testAction = new ConcreteTestAction(action);

        testAction.SetName(GetName());
        testAction.SetDescription(GetDescription());

        return testAction;
    }

    /// Concrete implementation of AbstractTestAction.
    /// This class is responsible for executing a delegate instance of ITestAction.
    private class ConcreteTestAction(TestAction delegateInstance) : AbstractTestAction
    {
        public override void Execute(TestContext context)
        {
            delegateInstance?.Invoke(context);
        }

        public override void DoExecute(TestContext context)
        {
            Execute(context);
        }
    }
}
