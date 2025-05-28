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

namespace Agenix.Api.Validation.Matcher;

/// <summary>
///     Control expression parser for extracting the individual control values from a control expression.
/// </summary>
public interface IControlExpressionParser
{
    /// <summary>
    ///     Some validation matchers can optionally contain one or more control values nested within a control expression
    ///     Matchers implementing this interface should take care of extracting the individual control values from the
    ///     expression. <br />
    ///     For example, expects between 2 and 3 control values(dateFrom, dateTo and optionally datePattern) to be
    ///     provided.It's
    ///     ControlExpressionParser would be expected to parse the control expression, returning each individual
    ///     control///value for each nested parameter found within the control expression.
    /// </summary>
    /// <param name="controlExpression">the control expression to be parsed</param>
    /// <param name="delimiter">the delimiter to use. When {@literal NULL} the {@link #DEFAULT_DELIMITER} is assumed.</param>
    /// <returns></returns>
    List<string> ExtractControlValues(string controlExpression, char delimiter);
}
