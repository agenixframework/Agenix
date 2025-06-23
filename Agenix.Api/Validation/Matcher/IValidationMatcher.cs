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
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Validation.Matcher;

/// <summary>
///     Represents an interface for implementing validation matchers in the validation framework.
/// </summary>
public interface IValidationMatcher
{
    /// <summary>
    ///     A static dictionary that stores a collection of validation matchers identified by their respective string keys.
    /// </summary>
    /// <remarks>
    ///     This variable serves as a central repository for registering and retrieving instances of classes
    ///     that provide validation logic, implementing the <see cref="IValidationMatcher" /> interface.
    ///     It supports dynamic resolution and reuse of validation matchers within the system.
    /// </remarks>
    private static readonly IDictionary<string, IValidationMatcher> Validators =
        new Dictionary<string, IValidationMatcher>();

    /// <summary>
    ///     A static instance of the logging mechanism used for capturing and managing log events within the system.
    /// </summary>
    /// <remarks>
    ///     This variable is initialized using the Log4Net logging framework and is primarily used to log diagnostic
    ///     and operational information to assist with debugging and monitoring of the validation processes.
    ///     It is tied to the <see cref="IValidationMatcher" /> interface for context-specific logging.
    ///     Typical use cases include logging validation events, warnings, errors, or informational messages.
    /// </remarks>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IValidationMatcher));

    /// <summary>
    ///     A static instance of the <c>ResourcePathTypeResolver</c> used for resolving resource paths
    ///     and retrieving type-related information during runtime.
    /// </summary>
    /// <remarks>
    ///     This resolver provides methods to dynamically locate and instantiate types or resources
    ///     based on a specified resource path and configuration. It is used for tasks like
    ///     loading type implementations or validating types in context-specific scenarios.
    /// </remarks>
    static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

    /// <summary>
    ///     Represents the path used to identify and locate the resource associated with the ValueMatcher implementation.
    /// </summary>
    static string ResourcePath => "Extension/agenix/validation/matcher";

    /// <summary>
    ///     Method called on validation.
    /// </summary>
    /// <param name="fieldName">the fieldName for logging purpose</param>
    /// <param name="value">the value to be validated.</param>
    /// <param name="controlParameters">the control parameters.</param>
    /// <param name="context"></param>
    void Validate(string fieldName, string value, List<string>? controlParameters, TestContext context);

    static IDictionary<string, IValidationMatcher> Lookup()
    {
        if (Validators.Count != 0)
        {
            return Validators;
        }

        var resolvedValidators = TypeResolver.ResolveAll<dynamic>();
        foreach (var kvp in resolvedValidators)
        {
            Validators[kvp.Key] = kvp.Value;
        }

        if (!Log.IsEnabled(LogLevel.Debug))
        {
            return Validators;
        }

        {
            foreach (var kvp in Validators)
            {
                Log.LogDebug("Found validation matcher '{KvpKey}' as {Type}", kvp.Key, kvp.Value.GetType());
            }
        }
        return Validators;
    }
}
