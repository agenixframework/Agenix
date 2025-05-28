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

namespace Agenix.Api;

/// Interface for running test actions.
/// /
public interface ITestActionRunner
{
    /// Runs the provided test action.
    /// @param action The test action to be executed.
    /// @return The executed test action.
    /// /
    T Run<T>(T action) where T : ITestAction;

    /// Runs the provided test action.
    /// @param action The test action to be executed.
    /// @return The executed test action.
    T Run<T>(Func<T> action) where T : ITestAction;

    /// Runs a given test action.
    /// @param action The test action to run.
    /// @return The result of the test action.
    /// /
    T Run<T>(ITestActionBuilder<T> builder) where T : ITestAction;

    /// Applies a specified test behavior using the given behavior interface.
    /// <param name="behavior">The test behavior to be applied.</param>
    /// <return>A builder for constructing the ApplyTestBehaviorAction with the specified behavior.</return>
    ITestActionBuilder<ITestAction> ApplyBehavior(ITestBehavior behavior);

    /// Applies a specified test behavior to the test action runner.
    /// <param name="behavior">The test behavior to be applied.</param>
    /// <return>A builder for creating a test action with the applied behavior.</return>
    ITestActionBuilder<ITestAction> ApplyBehavior(TestBehavior behavior);
}
