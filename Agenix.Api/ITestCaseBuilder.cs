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

using Agenix.Core;

namespace Agenix.Api;

/// Interface for building test cases with various configurations.
/// /
public interface ITestCaseBuilder
{
    /// Builds the test case.
    /// @return
    /// /
    ITestCase GetTestCase();

    /// Sets the test class.
    /// <param name="type">The Type object representing the test class to be set.</param>
    void SetTestClass(Type type);

    /// Sets a custom test case name.
    /// <param name="name">The name to be set for the test case.</param>
    void SetName(string name);

    /// Adds description to the test case.
    /// @param description The description to be set for the test case.
    /// /
    void SetDescription(string description);

    /// Adds author to the test case.
    /// @param author The name of the author to be added to the test case.
    /// /
    void SetAuthor(string author);

    /// Sets a custom package name for this test case.
    /// @param packageName The name of the package to be set.
    /// /
    void SetNamespaceName(string packageName);

    /// Sets the test case status.
    /// @param status The status to be set for the test case.
    /// /
    void SetStatus(TestCaseMetaInfo.Status status);

    /// Sets the creation date.
    /// @param date The DateTime object representing the creation date to be set.
    /// /
    void SetCreationDate(DateTime date);

    /// Sets the test group names for this test case.
    /// <param name="groups">An array of strings representing the names of the test groups to be set.</param>
    void SetGroups(string[] groups);

    /// Adds a new variable definition to the set of test variables
    /// for this test case and returns its value.
    /// @param name The name of the variable to set.
    /// @param value The value to be assigned to the variable.
    /// @return The value assigned to the variable.
    /// /
    T SetVariable<T>(string name, T value);
}
