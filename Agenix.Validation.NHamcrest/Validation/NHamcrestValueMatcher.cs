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
using Agenix.Api.Validation;
using NHamcrest;
using NHamcrest.Core;

namespace Agenix.Validation.NHamcrest.Validation;

/// <summary>
///     Class HamcrestValueMatcher provides validation logic for matching received objects
///     against control objects using Hamcrest matchers.
/// </summary>
public class NHamcrestValueMatcher : IValueMatcher
{
    /// <summary>
    ///     Filter supported value types
    /// </summary>
    /// <param name="controlType">The control type to be checked for support.</param>
    /// <returns>True if the control type is supported, otherwise false.</returns>
    public bool Supports(Type controlType)
    {
        var openGenericType = typeof(IMatcher<>);
        var result = controlType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericType);

        return result;
    }

    /// <summary>
    ///     Validates whether the received object matches the control object.
    /// </summary>
    /// <param name="received">The object to be validated.</param>
    /// <param name="control">The object used as the control for comparison.</param>
    /// <param name="context">The test context that provides additional validation context.</param>
    /// <returns>True if the received object matches the control, otherwise false.</returns>
    public bool Validate(object received, object control, TestContext context)
    {
        // workaround to be able to test a list of strings
        if (received is string receivedAsString)
            if (receivedAsString.Contains(','))
                received = receivedAsString.Split([','], StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()) // Remove extra spaces
                    .ToList();


        if (control != null)
        {
            var controlType = control.GetType();

            // Find the IMatcher<T> interface implemented by control
            var matcherInterface = controlType.GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IMatcher<>));

            if (matcherInterface != null)
            {
                var matchesMethod = matcherInterface.GetMethod("Matches");
                if (matchesMethod != null) return (bool)matchesMethod.Invoke(control, [received])!;
            }
        }

        var equalMatcher = new IsEqualMatcher<object>(control);
        return equalMatcher.Matches(received);
    }
}
