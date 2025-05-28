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

using System.Collections;
using System.Runtime.CompilerServices;

namespace Agenix.Api.Util;

/// <summary>
///     Assertion utility methods that simplify things such as argument checks.
/// </summary>
/// <remarks>
///     <p>
///         Not intended to be used directly by applications.
///     </p>
/// </remarks>
public static class AssertUtils
{
    /// <summary>
    ///     Checks the value of the supplied <paramref name="argument" /> and throws an
    ///     <see cref="System.ArgumentNullException" /> if it is <see langword="null" />.
    /// </summary>
    /// <param name="argument">The object to check.</param>
    /// <param name="name">The argument name.</param>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="argument" /> is <see langword="null" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ArgumentNotNull(object argument, string name)
    {
        if (argument == null) ThrowArgumentNullException(name);
    }

    /// <summary>
    ///     Checks the value of the supplied <paramref name="argument" /> and throws an
    ///     <see cref="System.ArgumentNullException" /> if it is <see langword="null" />.
    /// </summary>
    /// <param name="argument">The object to check.</param>
    /// <param name="name">The argument name.</param>
    /// <param name="message">
    ///     An arbitrary message that will be passed to any thrown
    ///     <see cref="System.ArgumentNullException" />.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="argument" /> is <see langword="null" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ArgumentNotNull(object argument, string name, string message)
    {
        if (argument == null) ThrowArgumentNullException(name, message);
    }

    /// <summary>
    ///     Checks the value of the supplied string <paramref name="argument" /> and throws an
    ///     <see cref="System.ArgumentNullException" /> if it is <see langword="null" /> or
    ///     contains only whitespace character(s).
    /// </summary>
    /// <param name="argument">The string to check.</param>
    /// <param name="name">The argument name.</param>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="argument" /> is <see langword="null" /> or
    ///     contains only whitespace character(s).
    /// </exception>
    public static void ArgumentHasText(string argument, string name)
    {
        if (StringUtils.IsNullOrEmpty(argument))
            ThrowArgumentNullException(
                name,
                $"Argument '{name}' cannot be null or resolve to an empty string : '{argument}'.");
    }

    /// <summary>
    ///     Checks the value of the supplied string <paramref name="argument" /> and throws an
    ///     <see cref="System.ArgumentNullException" /> if it is <see langword="null" /> or
    ///     contains only whitespace character(s).
    /// </summary>
    /// <param name="argument">The string to check.</param>
    /// <param name="name">The argument name.</param>
    /// <param name="message">
    ///     An arbitrary message that will be passed to any thrown
    ///     <see cref="System.ArgumentNullException" />.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="argument" /> is <see langword="null" /> or
    ///     contains only whitespace character(s).
    /// </exception>
    public static void ArgumentHasText(string argument, string name, string message)
    {
        if (StringUtils.IsNullOrEmpty(argument)) ThrowArgumentNullException(name, message);
    }

    /// <summary>
    ///     Checks the value of the supplied <see cref="ICollection" /> <paramref name="argument" /> and throws
    ///     an <see cref="ArgumentNullException" /> if it is <see langword="null" /> or contains no elements.
    /// </summary>
    /// <param name="argument">The array or collection to check.</param>
    /// <param name="name">The argument name.</param>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="argument" /> is <see langword="null" /> or
    ///     contains no elements.
    /// </exception>
    public static void ArgumentHasLength(ICollection argument, string name)
    {
        if (!ArrayUtils.HasLength(argument))
            ThrowArgumentNullException(
                name,
                $"Argument '{name}' cannot be null or resolve to an empty array");
    }

    /// <summary>
    ///     Checks the value of the supplied <see cref="ICollection" /> <paramref name="argument" /> and throws
    ///     an <see cref="ArgumentNullException" /> if it is <see langword="null" /> or contains no elements.
    /// </summary>
    /// <param name="argument">The array or collection to check.</param>
    /// <param name="name">The argument name.</param>
    /// <param name="message">
    ///     An arbitrary message that will be passed to any thrown
    ///     <see cref="System.ArgumentNullException" />.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="argument" /> is <see langword="null" /> or
    ///     contains no elements.
    /// </exception>
    public static void ArgumentHasLength(ICollection argument, string name, string message)
    {
        if (!ArrayUtils.HasLength(argument)) ThrowArgumentNullException(name, message);
    }

    /// <summary>
    ///     Checks the value of the supplied <see cref="ICollection" /> <paramref name="argument" /> and throws
    ///     an <see cref="ArgumentException" /> if it is <see langword="null" />, contains no elements or only null elements.
    /// </summary>
    /// <param name="argument">The array or collection to check.</param>
    /// <param name="name">The argument name.</param>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="argument" /> is <see langword="null" />,
    ///     contains no elements or only null elements.
    /// </exception>
    public static void ArgumentHasElements(ICollection argument, string name)
    {
        if (!ArrayUtils.HasElements(argument))
            ThrowArgumentException(
                name,
                $"Argument '{name}' must not be null or resolve to an empty collection and must contain non-null elements");
    }

    /// <summary>
    ///     Checks whether the specified <paramref name="argument" /> can be cast
    ///     into the <paramref name="requiredType" />.
    /// </summary>
    /// <param name="argument">
    ///     The argument to check.
    /// </param>
    /// <param name="argumentName">
    ///     The name of the argument to check.
    /// </param>
    /// <param name="requiredType">
    ///     The required type for the argument.
    /// </param>
    /// <param name="message">
    ///     An arbitrary message that will be passed to any thrown
    ///     <see cref="System.ArgumentException" />.
    /// </param>
    public static void AssertArgumentType(object argument, string argumentName, Type requiredType, string message)
    {
        if (argument != null && requiredType != null && !requiredType.IsInstanceOfType(argument))
            ThrowArgumentException(message, argumentName);
    }

    /// <summary>
    ///     Assert a boolean expression, throwing <code>ArgumentException</code>
    ///     if the test result is <code>false</code>.
    /// </summary>
    /// <param name="expression">a boolean expression.</param>
    /// <param name="message">The exception message to use if the assertion fails.</param>
    /// <exception cref="ArgumentException">
    ///     if expression is <code>false</code>
    /// </exception>
    public static void IsTrue(bool expression, string message)
    {
        if (!expression) ThrowArgumentException(message);
    }

    /// <summary>
    ///     Assert a boolean expression, throwing <code>ArgumentException</code>
    ///     if the test result is <code>false</code>.
    /// </summary>
    /// <param name="expression">a boolean expression.</param>
    /// <exception cref="ArgumentException">
    ///     if expression is <code>false</code>
    /// </exception>
    public static void IsTrue(bool expression)
    {
        IsTrue(expression, "[Assertion failed] - this expression must be true");
    }

    /// <summary>
    ///     Assert a bool expression, throwing <code>InvalidOperationException</code>
    ///     if the expression is <code>false</code>.
    /// </summary>
    /// <param name="expression">a boolean expression.</param>
    /// <param name="message">The exception message to use if the assertion fails</param>
    /// <exception cref="InvalidOperationException">if expression is <code>false</code></exception>
    public static void State(bool expression, string message)
    {
        if (!expression) ThrowInvalidOperationException(message);
    }

    private static void ThrowInvalidOperationException(string message)
    {
        throw new InvalidOperationException(message);
    }

    private static void ThrowNotSupportedException(string message)
    {
        throw new NotSupportedException(message);
    }

    private static void ThrowArgumentNullException(string paramName)
    {
        throw new ArgumentNullException(paramName, $"Argument '{paramName}' cannot be null.");
    }

    private static void ThrowArgumentNullException(string paramName, string message)
    {
        throw new ArgumentNullException(paramName, message);
    }

    private static void ThrowArgumentException(string message)
    {
        throw new ArgumentException(message);
    }

    private static void ThrowArgumentException(string message, string paramName)
    {
        throw new ArgumentException(message, paramName);
    }
}
