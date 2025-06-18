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

namespace Agenix.Api.Variable;

/// <summary>
///     Class extracting values of segments of VariableExpressions.
/// </summary>
public interface ISegmentVariableExtractor
{
    /// <summary>
    ///     Extract variables from given object.
    /// </summary>
    /// <param name="testContext">the test context</param>
    /// <param name="obj">the object of which to extract the value</param>
    /// <param name="matcher"></param>
    bool CanExtract(TestContext testContext, object obj, VariableExpressionSegmentMatcher matcher);

    /// <summary>
    ///     Extract variables from a given object. Implementations should throw a AgenixSystemException
    /// </summary>
    /// <param name="testContext">the test context</param>
    /// <param name="obj">the object of which to extract the value</param>
    /// <param name="matcher"></param>
    object ExtractValue(TestContext testContext, object obj, VariableExpressionSegmentMatcher matcher);
}
