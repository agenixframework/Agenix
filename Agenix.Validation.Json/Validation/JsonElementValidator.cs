#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Context;
using Agenix.Api.Validation.Matcher;
using Agenix.Core.Validation;
using Newtonsoft.Json.Linq;

namespace Agenix.Validation.Json.Validation;

/// <summary>
///     JsonElementValidator is responsible for validating JSON elements against expected structures and values.
/// </summary>
public class JsonElementValidator(bool strict, TestContext context, ICollection<string> ignoreExpressions)
{
    /// <summary>
    ///     Validates a given JSON element by checking its actual and expected structure and values.
    /// </summary>
    /// <param name="control">The control item containing the actual and expected JSON elements to validate.</param>
    /// <exception cref="ValidationException">Thrown when the JSON structure or values do not match the expected behavior.</exception>
    public virtual void Validate(JsonElementValidatorItem<object> control)
    {
        if (IsIgnoredByPlaceholderOrExpressionList(ignoreExpressions, control))
        {
            return;
        }

        if (ValidationMatcherUtils.IsValidationMatcherExpression(control.ExpectedAsStringOrNull() ?? ""))
        {
            ValidationMatcherUtils.ResolveValidationMatcher(control.GetJsonPath(), control.ActualAsStringOrNull(),
                control.ExpectedAsStringOrNull(), context);
        }

        else if (control._expected.GetType() == typeof(JObject))
        {
            ValidateJsonObject(this, control);
        }
        else if (control._expected.GetType() == typeof(JArray))
        {
            ValidateJsonArray(this, control);
        }
        else
        {
            ValidateNativeType(control);
        }
    }

    public void ValidateJsonArray(JsonElementValidator validator, JsonElementValidatorItem<object> control)
    {
        var arrayControl = control.EnsureType<JArray>();

        if (strict)
        {
            ValidateSameSize(control.GetJsonPath(), arrayControl._expected, arrayControl._actual);
        }

        var actualIndex = 0;
        for (var i = 0; i < arrayControl._expected.Count; i++)
        {
            if (IsIgnoredByPlaceholderOrExpressionList(ignoreExpressions, arrayControl.Child(i, i)))
            {
                continue;
            }

            var isValid = false;
            while (!isValid && actualIndex < arrayControl._actual.Count)
            {
                var item = arrayControl.Child(i, arrayControl._actual[actualIndex]);
                isValid = IsValidItem(item, validator);
                actualIndex++;
            }

            if (!isValid)
            {
                throw new ValidationException(ValidationUtils.BuildValueToBeInCollectionErrorMessage(
                    $"An item in '{arrayControl.GetJsonPath()}' is missing",
                    arrayControl._expected[i].ToString(),
                    arrayControl._actual.ToArray()));
            }
        }
    }

    /// <summary>
    ///     Determines if a given JSON element validator item is valid according to specified validation rules.
    /// </summary>
    /// <param name="validatorItem">The item containing the actual and expected JSON elements to be validated.</param>
    /// <param name="validator">The validator instance to be used for performing the validation.</param>
    /// <returns>True if the item is valid; otherwise, false.</returns>
    private static bool IsValidItem(JsonElementValidatorItem<object> validatorItem, JsonElementValidator validator)
    {
        try
        {
            validator.Validate(validatorItem);
            return true;
        }
        catch (ValidationException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Validates a JSON object by comparing its actual and expected structure and values, throwing an exception if they do
    ///     not match.
    /// </summary>
    /// <param name="validator">The JSON element validator instance used for validation operations.</param>
    /// <param name="control">The control item containing the actual and expected JSON objects to validate.</param>
    /// <exception cref="ValidationException">Thrown when the JSON structure or values do not match the expected behavior.</exception>
    private void ValidateJsonObject(JsonElementValidator validator, JsonElementValidatorItem<object> control)
    {
        var objectControl = control.EnsureType<JObject>();

        if (strict)
        {
            var expectedKeys = objectControl._expected.Properties().Select(p => p.Name).ToList();
            var actualKeys = objectControl._actual.Properties().Select(p => p.Name).ToList();
            ValidateSameSize(objectControl.GetJsonPath(), expectedKeys, actualKeys);
        }

        var controlEntries = objectControl._expected.Properties()
            .Select(entry =>
            {
                var item = new JsonElementValidatorItem<object>(
                    entry.Name, objectControl._actual.GetValue(entry.Name), entry.Value);
                item.Parent(control);
                return item;
            })
            .ToList();

        foreach (var entryControl in controlEntries)
        {
            if (!objectControl._actual.ContainsKey(entryControl.GetName()))
            {
                throw new ValidationException(ValidationUtils.BuildValueToBeInCollectionErrorMessage(
                    "Missing JSON entry", entryControl.GetName(),
                    objectControl._actual.Properties().Select(p => p.Name).ToList()
                ));
            }

            validator.Validate(entryControl);
        }
    }


    /// <summary>
    ///     Validates that the sizes of the expected and actual collections are the same.
    /// </summary>
    /// <param name="path">The JSON path being validated.</param>
    /// <param name="expected">The expected collection.</param>
    /// <param name="actual">The actual collection.</param>
    /// <exception cref="ValidationException">Thrown when the sizes of the expected and actual collections do not match.</exception>
    private void ValidateSameSize<T>(string path, ICollection<T> expected, ICollection<T> actual)
    {
        if (expected.Count != actual.Count)
        {
            ThrowValueMismatch($"Number of entries is not equal in element: '{path}'", string.Join(", ", expected),
                string.Join(", ", actual));
        }
    }

    /// <summary>
    ///     Validates that the actual value of a JSON element matches the expected value.
    /// </summary>
    /// <param name="control">The control item containing the expected and actual values, as well as metadata for validation.</param>
    /// <exception cref="ValidationException">Thrown when the actual value does not match the expected value.</exception>
    private static void ValidateNativeType(JsonElementValidatorItem<object> control)
    {
        if (!Equals(control._expected, control._actual))
        {
            ThrowValueMismatch($"Values not equal for entry: '{control.GetJsonPath()}'", control._expected,
                control._actual);
        }
    }

    /// <summary>
    ///     Throws a <see cref="ValidationException" /> indicating a mismatch between expected and actual values.
    /// </summary>
    /// <param name="baseMessage">The base message for the exception.</param>
    /// <param name="expectedValue">The expected value that was not matched.</param>
    /// <param name="actualValue">The actual value that did not match the expected value.</param>
    /// <exception cref="ValidationException">Thrown when there is a mismatch between the expected and actual values.</exception>
    private static void ThrowValueMismatch(string baseMessage, object expectedValue, object actualValue)
    {
        throw new ValidationException(
            ValidationUtils.BuildValueMismatchErrorMessage(baseMessage, expectedValue, actualValue));
    }

    /// <summary>
    ///     Checks if a control entry is ignored based on placeholders or a list of ignored expressions.
    /// </summary>
    /// <param name="ignoreExpressions">A collection of ignored expressions.</param>
    /// <param name="controlEntry">The control entry to be evaluated.</param>
    /// <returns>
    ///     True if the control entry is ignored; otherwise, false.
    /// </returns>
    public static bool IsIgnoredByPlaceholderOrExpressionList(ICollection<string> ignoreExpressions,
        JsonElementValidatorItem<object> controlEntry)
    {
        var trimmedControlValue = (controlEntry.ExpectedAsStringOrNull() ?? string.Empty).Trim();
        return trimmedControlValue.Equals(AgenixSettings.IgnorePlaceholder) ||
               ignoreExpressions.Any(controlEntry.IsPathIgnoredBy);
    }

    /// <summary>
    ///     DefaultProvider implements the IProvider interface and is responsible for providing instances of
    ///     JsonElementValidator
    ///     based on given configuration parameters.
    /// </summary>
    public class DefaultProvider : IProvider
    {
        /// <summary>
        ///     Retrieves a JsonElementValidator instance configured with the specified parameters.
        /// </summary>
        /// <param name="isStrict">Defines whether the validator should operate in strict mode.</param>
        /// <param name="context">
        ///     The context in which the validation is taking place, providing necessary test-related data and
        ///     functionality.
        /// </param>
        /// <param name="validationContext">Context containing additional validation settings, including ignored expressions.</param>
        /// <returns>A new instance of JsonElementValidator configured according to the provided parameters.</returns>
        public JsonElementValidator GetValidator(bool isStrict, TestContext context,
            IMessageValidationContext validationContext)
        {
            return new JsonElementValidator(isStrict, context, validationContext.IgnoreExpressions);
        }
    }
}
