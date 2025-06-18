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

namespace Agenix.Api.Builder;

/// <summary>
///     Provides a fluent interface for defining and managing expressions to be evaluated.
///     Implementations of this interface allow the configuration of expressions and their corresponding
///     target variable names for storing evaluation results in a context.
/// </summary>
/// <typeparam name="TB">
///     The type that implements this interface, enabling method chaining in a fluent API style.
/// </typeparam>
public interface IWithExpressions<out TB>
{
    /// <summary>
    ///     Sets the expressions to evaluate. Keys are expressions that should be evaluated, and values are target
    ///     variable names that are stored in the test context with the evaluated result as a variable value.
    /// </summary>
    /// <param name="expressions">A dictionary of expressions and their corresponding target variable names</param>
    /// <returns>The type that implements this interface</returns>
    TB Expressions(IDictionary<string, object> expressions);

    /// <summary>
    ///     Adds an expression that gets evaluated. The evaluation result is stored in the test context as a variable
    ///     with the given variable name.
    /// </summary>
    /// <param name="expression">The expression to be evaluated</param>
    /// <param name="value">The target variable name</param>
    /// <returns>The type that implements this interface</returns>
    TB Expression(string expression, object value);
}
