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


using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Api.Validation.Matcher;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Validation.Matcher.Core;

/// <summary>
///     The <c>CreateVariableValidationMatcher</c> class is responsible for validating and setting variable values
///     within a given <see cref="TestContext" /> during the validation process.
/// </summary>
/// <remarks>
///     This class implements the <see cref="IValidationMatcher" /> interface and provides functionality
///     for setting variables based on the provided field name and value. Optionally, a list of control
///     parameters can be used to customize the variable name.
/// </remarks>
/// <example>
///     This class is designed to integrate with the validation framework and is not used directly,
///     but rather utilized during validation workflows to handle specific variable mappings.
/// </example>
/// <seealso cref="IValidationMatcher" />
public class CreateVariableValidationMatcher : IValidationMatcher
{
    /// <summary>Logger</summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(CreateVariableValidationMatcher));

    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        var name = fieldName;

        if (controlParameters != null && controlParameters.Count > 0)
        {
            name = controlParameters[0];
        }

        Log.LogDebug("Setting variable: {Name} to value: {Value}", name, value);

        context.SetVariable(name, value);
    }
}
