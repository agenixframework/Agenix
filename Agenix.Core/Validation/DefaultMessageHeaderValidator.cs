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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Agenix.Api.Context;
using Agenix.Api.Endpoint.Resolver;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Validation;

/// <summary>
///     Basic header message validator provides message header validation. Subclasses only have to add specific logic for
///     message payload validation. This validator is based on a control message.
/// </summary>
public class DefaultMessageHeaderValidator : AbstractMessageValidator<HeaderValidationContext>,
    IMessageValidator<IValidationContext>
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(DefaultMessageHeaderValidator));

    /// <summary>
    ///     A dictionary containing the set of default header validators located via resource path lookup.
    /// </summary>
    private readonly IDictionary<string, IHeaderValidator> _defaultValidators = IHeaderValidator.Lookup();

    /// <summary>
    ///     List of special header validators.
    /// </summary>
    private List<IHeaderValidator> _validators = [];

    public DefaultMessageHeaderValidator() { }

    /// <summary>
    ///     Determines if the given message type is supported by this message validator.
    /// </summary>
    /// <param name="messageType">The type of the message to be validated.</param>
    /// <param name="message">The message instance to be validated.</param>
    /// <returns>True if the message type is supported, otherwise false.</returns>
    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return true;
    }

    /// <summary>
    ///     Validates the headers of a received message against the headers of a control message
    ///     within the specified validation and test contexts.
    /// </summary>
    /// <param name="receivedMessage">The received message whose headers are being validated.</param>
    /// <param name="controlMessage">The control message containing the expected header values.</param>
    /// <param name="context">The test context providing additional test-related information.</param>
    /// <param name="validationContext">The validation context specific to header validation.</param>
    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        HeaderValidationContext validationContext)
    {
        IDictionary<string, object> controlHeaders = controlMessage.GetHeaders();
        IDictionary<string, object> receivedHeaders = receivedMessage.GetHeaders();

        if (controlHeaders == null || !controlHeaders.Any())
        {
            return;
        }

        Log.LogDebug("Start message header validation ...");

        foreach (var (key, controlValue) in controlHeaders)
        {
            if (MessageHeaderUtils.IsSpringInternalHeader(key) ||
                key.StartsWith(MessageHeaders.MessagePrefix) ||
                key.Equals(IEndpointUriResolver.EndpointUriHeaderName) ||
                key.Equals(IEndpointUriResolver.RequestPathHeaderName) ||
                key.Equals(IEndpointUriResolver.QueryParamHeaderName))
            {
                continue;
            }

            var headerName = GetHeaderName(key, receivedHeaders, context, validationContext);

            if (!receivedHeaders.TryGetValue(headerName, out var value))
            {
                throw new ValidationException($"Validation failed: Header element '{headerName}' is missing");
            }

            var validator = validationContext.Validators
                                .FirstOrDefault(v => v.Supports(headerName, controlValue?.GetType()))
                            ?? validationContext.ValidatorNames
                                .Select(beanName =>
                                {
                                    try
                                    {
                                        return context.ReferenceResolver.Resolve<IHeaderValidator>(beanName);
                                    }
                                    catch (AgenixSystemException)
                                    {
                                        Log.LogWarning($"Failed to resolve header validator for name: {beanName}");
                                        return null;
                                    }
                                })
                                .FirstOrDefault(v => v != null && v.Supports(headerName, controlValue?.GetType()))
                            ?? GetHeaderValidators(context)
                                .FirstOrDefault(v => v.Supports(headerName, controlValue?.GetType()))
                            ?? new DefaultHeaderValidator();

            validator.ValidateHeader(headerName, value, controlValue, context, validationContext);
        }

        Log.LogDebug("Message header validation successful: All values OK");
    }

    /// <summary>
    ///     Retrieves a list of header validators that are available in the given test context.
    /// </summary>
    /// <param name="context">The context of the test which may influence the selection of validators.</param>
    /// <returns>A list of distinct header validators available in the test context.</returns>
    private List<IHeaderValidator> GetHeaderValidators(TestContext context)
    {
        // add validators from the resource path lookup
        var validatorMap = new Dictionary<string, IHeaderValidator>(_defaultValidators);

        var validators = context.ReferenceResolver.ResolveAll<IHeaderValidator>();

        if (validators != null && validators.Count > 0)
        {
            foreach (var validator in validators)
            {
                validatorMap.TryAdd(validator.Key, validator.Value);
            }
        }

        return validatorMap.Values.Distinct().ToList();
    }

    /// <summary>
    ///     Retrieves the appropriate header name, potentially performing a case-insensitive search if configured.
    /// </summary>
    /// <param name="name">The name of the header to be resolved.</param>
    /// <param name="receivedHeaders">A collection of headers received in the message.</param>
    /// <param name="context">The context of the test, providing auxiliary functions and variables.</param>
    /// <param name="validationContext">The context for header validation, including settings such as case sensitivity.</param>
    /// <returns>The resolved header name, potentially found in a case-insensitive manner.</returns>
    private static string GetHeaderName(string name, IDictionary<string, object> receivedHeaders, TestContext context,
        HeaderValidationContext validationContext)
    {
        var headerName = context.ResolveDynamicValue(name);

        if (receivedHeaders.ContainsKey(headerName) || !validationContext.HeaderNameIgnoreCase)
        {
            return headerName;
        }

        var key = headerName;

        Log.LogDebug($"Finding case insensitive header for key '{key}'");

        headerName = receivedHeaders
                         .AsParallel()
                         .Where(item => item.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                         .Select(item => item.Key)
                         .FirstOrDefault() ??
                     throw new ValidationException($"Validation failed: No matching header for key '{key}'");

        Log.LogDebug($"Found matching case insensitive header name: {headerName}");

        return headerName;
    }


    /// <summary>
    ///     Returns the required type of validation context for this message validator.
    /// </summary>
    /// <returns>The <see cref="Type" /> of the required validation context.</returns>
    protected override Type GetRequiredValidationContextType()
    {
        return typeof(HeaderValidationContext);
    }

    /// <summary>
    ///     Adds a header validator to the list.
    /// </summary>
    /// <param name="validator">The <see cref="IHeaderValidator" /> to add.</param>
    public void AddHeaderValidator(IHeaderValidator validator)
    {
        _validators.Add(validator);
    }

    /// <summary>
    ///     Retrieves the list of header validators.
    /// </summary>
    /// <returns>An unmodifiable list of <see cref="IHeaderValidator" />s.</returns>
    public IReadOnlyList<IHeaderValidator> GetValidators()
    {
        return new ReadOnlyCollection<IHeaderValidator>(_validators);
    }

    /// <summary>
    ///     Sets the list of header validators.
    /// </summary>
    /// <param name="validators">The list of <see cref="IHeaderValidator" />s to set.</param>
    public void SetValidators(List<IHeaderValidator> validators)
    {
        _validators = validators;
    }
}
