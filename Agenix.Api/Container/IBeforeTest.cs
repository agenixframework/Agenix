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

namespace Agenix.Api.Container;

/// <summary>
///     Represents an interface that defines a set of actions to be performed before test execution.
/// </summary>
public interface IBeforeTest : ITestActionContainer
{
    /// <summary>
    ///     Checks if this suite action should execute, according to suite name and included test groups.
    /// </summary>
    /// <param name="testName">The name of the test.</param>
    /// <param name="packageName">The name of the package.</param>
    /// <param name="includedGroups">The groups of tests included.</param>
    /// <returns>Returns true if the actions should execute; otherwise false.</returns>
    bool ShouldExecute(string testName, string packageName, string[] includedGroups);
}
