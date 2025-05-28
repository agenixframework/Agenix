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

/// <summary>
///     Interface that marks a test case to support group bindings.
///     These bindings are used to enable or disable tests based on group settings.
/// </summary>
public interface ITestGroupAware
{
    /// <summary>
    ///     Gets the groups associated with the test case.
    /// </summary>
    /// <returns>
    ///     An array of strings representing the groups.
    /// </returns>
    string[] GetGroups();

    /// <summary>
    ///     Sets the test groups associated with the test case.
    /// </summary>
    /// <param name="groups">
    ///     An array of strings representing the groups to be associated with the test case.
    /// </param>
    void SetGroups(string[] groups);
}
