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

namespace Agenix.Api.Condition;

/// Tests whether a condition is satisfied.
public interface ICondition
{
    /// Gets the condition name.
    /// @return The name of the condition.
    /// /
    string GetName();

    /// Tests the condition and returns true if it is satisfied.
    /// <param name="context">The test context in which the condition is evaluated.</param>
    /// <return>True if the condition is satisfied; otherwise, false.</return>
    bool IsSatisfied(TestContext context);

    /// Constructs a success message for the current condition.
    /// <param name="context">The test context used to evaluate the condition.</param>
    /// <return>A success message indicating that the condition has been satisfied.</return>
    string GetSuccessMessage(TestContext context);

    /// Constructs an error message for the condition.
    /// <param name="context">The test context related to the condition.</param>
    /// <return>The error message specific to this condition.</return>
    string GetErrorMessage(TestContext context);
}
