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

using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Api.Validation.Matcher;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Text.Validation.Text;

/// <summary>
///     The PlainTextMessageValidator class is responsible for validating plain text messages.
///     It extends the DefaultMessageValidator and provides specific logic to determine
///     if a message of type plaintext is supported.
/// </summary>
public class PlainTextMessageValidator : DefaultMessageValidator
{
    public const string IgnoreNewlineTypeProperty = "agenix.plaintext.validation.ignore.newline.type";
    public const string IgnoreNewlineTypeEnv = "AGENIX_PLAINTEXT_VALIDATION_IGNORE_NEWLINE_TYPE";
    public const string IgnoreWhitespaceProperty = "agenix.plaintext.validation.ignore.whitespace";
    public const string IgnoreWhitespaceEnv = "AGENIX_PLAINTEXT_VALIDATION_IGNORE_WHITESPACE";

    private static readonly ILogger Log = LogManager.GetLogger(typeof(PlainTextMessageValidator));

    /// <summary>
    ///     Gets the value of ignoreWhitespace.
    /// </summary>
    public bool IgnoreWhitespace { get; set; } = bool.Parse(
        ConfigurationManager.AppSettings[IgnoreWhitespaceProperty] ??
        Environment.GetEnvironmentVariable(IgnoreWhitespaceEnv) ?? "false");

    /// <summary>
    ///     Gets the value of ignoreNewLineType.
    /// </summary>
    public bool IgnoreNewLineType { get; set; } = bool.Parse(
        ConfigurationManager.AppSettings[IgnoreNewlineTypeProperty] ??
        Environment.GetEnvironmentVariable(IgnoreNewlineTypeEnv) ?? "false");

    /// <summary>
    ///     Validates the received plaintext message against the expected control message.
    ///     This method normalizes whitespace, processes ignore statements, and performs
    ///     text validation against the control message within the provided context.
    /// </summary>
    /// <param name="receivedMessage">The received message that needs to be validated.</param>
    /// <param name="controlMessage">The control message containing the expected payload.</param>
    /// <param name="context">The test context containing dynamic content replacements.</param>
    /// <param name="validationContext">The validation context providing additional validation parameters.</param>
    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        IValidationContext validationContext)
    {
        if (controlMessage?.Payload == null)
        {
            Log.LogDebug("Skip message payload validation as no control message was defined");
            return;
        }

        Log.LogDebug("Start text message validation");

        try
        {
            var resultValue = NormalizeWhitespace(receivedMessage.GetPayload<string>().Trim());
            var controlValue =
                NormalizeWhitespace(context.ReplaceDynamicContentInString(controlMessage.GetPayload<string>().Trim()));

            controlValue = ProcessIgnoreStatements(controlValue, resultValue);
            controlValue = ProcessVariableStatements(controlValue, resultValue, context);

            if (ValidationMatcherUtils.IsValidationMatcherExpression(controlValue))
            {
                ValidationMatcherUtils.ResolveValidationMatcher("payload", resultValue, controlValue, context);
                return;
            }

            ValidateText(resultValue, controlValue);
        }
        catch (ArgumentException e)
        {
            throw new ValidationException("Failed to validate text content", e);
        }

        Log.LogDebug("Text validation successful: All values OK");
    }

    /// <summary>
    ///     Processes nested ignore statements in control value and replaces that ignore placeholder with the actual value at
    ///     this position. This way we can ignore words in a plaintext value.
    /// </summary>
    /// <param name="control"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private static string ProcessIgnoreStatements(string control, string result)
    {
        if (control.Equals(AgenixSettings.IgnorePlaceholder))
        {
            return control;
        }

        var whitespacePattern = new Regex(@"\W");
        var ignorePattern = new Regex(@"@Ignore\(?(\d*)\)?@");

        var ignoreMatcher = ignorePattern.Match(control);
        while (ignoreMatcher.Success)
        {
            string actualValue;

            if (ignoreMatcher.Groups.Count > 1 && !string.IsNullOrWhiteSpace(ignoreMatcher.Groups[1].Value))
            {
                var end = ignoreMatcher.Index + int.Parse(ignoreMatcher.Groups[1].Value);
                if (end > result.Length)
                {
                    end = result.Length;
                }

                actualValue = ignoreMatcher.Index > result.Length
                    ? ""
                    : result.Substring(ignoreMatcher.Index, end - ignoreMatcher.Index);
            }
            else
            {
                actualValue = result[ignoreMatcher.Index..];
                var whitespaceMatcher = whitespacePattern.Match(actualValue);
                if (whitespaceMatcher.Success)
                {
                    actualValue = actualValue[..whitespaceMatcher.Index];
                }
            }

            control = ignorePattern.Replace(control, actualValue, 1);
            ignoreMatcher = ignorePattern.Match(control);
        }

        return control;
    }

    /// <summary>
    ///     Processes nested ignore statements in control value and replaces that ignore placeholder with the actual value at
    ///     this position. This way we can ignore words in a plaintext value.
    /// </summary>
    /// <param name="control"></param>
    /// <param name="result"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private static string ProcessVariableStatements(string control, string result, TestContext context)
    {
        if (control.Equals(AgenixSettings.IgnorePlaceholder))
        {
            return control;
        }

        var whitespacePattern = new Regex(@"[^a-zA-Z_0-9\-\.]");
        var variablePattern = new Regex(@"@Variable\(?\'?([a-zA-Z_0-9\-\.]*)\'?\)?@");

        var variableMatcher = variablePattern.Match(control);
        while (variableMatcher.Success)
        {
            var actualValue = result[variableMatcher.Index..];
            var whitespaceMatcher = whitespacePattern.Match(actualValue);
            if (whitespaceMatcher.Success)
            {
                actualValue = actualValue[..whitespaceMatcher.Index];
            }

            control = variablePattern.Replace(control, actualValue, 1);
            context.SetVariable(variableMatcher.Groups[1].Value, actualValue);
            variableMatcher = variablePattern.Match(control);
        }

        return control;
    }

    /// <summary>
    ///     Validates the received message payload against the control message payload.
    ///     Checks if the received message payload matches the control message payload or,
    ///     if whitespace is ignored, matches after removing all whitespace characters.
    /// </summary>
    /// <param name="receivedMessagePayload">
    ///     The message payload that was received and needs to be validated.
    /// </param>
    /// <param name="controlMessagePayload">
    ///     The control message payload that the received message payload is validated against.
    /// </param>
    /// <exception cref="ValidationException">
    ///     Thrown when the received message payload does not match the control message payload,
    ///     either directly or after ignoring whitespace characters.
    /// </exception>
    private void ValidateText(string receivedMessagePayload, string controlMessagePayload)
    {
        if (string.IsNullOrWhiteSpace(controlMessagePayload))
        {
            Log.LogDebug("Skip message payload validation as no control message was defined");
            return;
        }

        if (string.IsNullOrWhiteSpace(receivedMessagePayload))
        {
            throw new ValidationException("Validation failed - expected message contents, but received empty message!");
        }

        if (!receivedMessagePayload.Equals(controlMessagePayload))
        {
            if (Regex.Replace(receivedMessagePayload, @"\s", "")
                .Equals(Regex.Replace(controlMessagePayload, @"\s", "")))
            {
                throw new ValidationException(
                    $"Text values not equal (only whitespaces!), expected '{controlMessagePayload}' but was '{receivedMessagePayload}'");
            }

            throw new ValidationException(
                $"Text values not equal, expected '{controlMessagePayload}' but was '{receivedMessagePayload}'");
        }
    }

    /// <summary>
    ///     Normalize whitespace characters if appropriate. Based on system property settings this method normalizes new line
    ///     characters exclusively or filters all whitespaces such as double whitespaces and new lines.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    private string NormalizeWhitespace(string payload)
    {
        if (IgnoreWhitespace)
        {
            var result = new StringBuilder();
            var lastWasSpace = true;
            foreach (var c in payload)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!lastWasSpace)
                    {
                        result.Append(' ');
                    }

                    lastWasSpace = true;
                }
                else
                {
                    result.Append(c);
                    lastWasSpace = false;
                }
            }

            return result.ToString().Trim();
        }

        return IgnoreNewLineType ? Regex.Replace(payload, @"\r(\n)?", "\n") : payload;
    }

    /// <summary>
    ///     Determines if the given message type is supported by the PlainTextMessageValidator.
    /// </summary>
    /// <param name="messageType">The type of the message to check.</param>
    /// <param name="message">The message instance to check for support.</param>
    /// <returns>True if the message type is supported, otherwise false.</returns>
    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return messageType.Equals(nameof(MessageType.PLAINTEXT), StringComparison.OrdinalIgnoreCase);
    }
}
