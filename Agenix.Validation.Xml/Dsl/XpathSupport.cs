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

using Agenix.Api.Builder;
using Agenix.Api.Message;
using Agenix.Api.Validation.Context;
using Agenix.Api.Variable;
using Agenix.Core.Validation.Xml;
using Agenix.Validation.Xml.Validation.Xml;

namespace Agenix.Validation.Xml.Dsl;

/// <summary>
///     Provides support for configuring and using XPath-related functionalities within the DSL.
///     This class serves as a bridge to configure expressions and adapt to various
///     processing contexts such as message processing, variable extraction, and validation.
/// </summary>
public class XpathSupport : IWithExpressions<XpathSupport>, IPathExpressionAdapter
{
    private readonly Dictionary<string, object> _expressions = new();

    public IMessageProcessor AsProcessor()
    {
        return new XpathMessageProcessor.Builder()
            .Expressions(_expressions)
            .Build();
    }

    public IVariableExtractor AsExtractor()
    {
        return new XpathPayloadVariableExtractor.Builder()
            .Expressions(_expressions)
            .Build();
    }

    public IValidationContext AsValidationContext()
    {
        return new XpathMessageValidationContext.Builder()
            .Expressions(_expressions)
            .Build();
    }

    public XpathSupport Expressions(IDictionary<string, object> expressions)
    {
        foreach (var kvp in expressions)
        {
            _expressions[kvp.Key] = kvp.Value;
        }

        return this;
    }

    public XpathSupport Expression(string expression, object value)
    {
        _expressions[expression] = value;
        return this;
    }

    /// <summary>
    ///     Static entrance for all XPath-related C# DSL functionalities.
    /// </summary>
    /// <returns>New XpathSupport instance</returns>
    public static XpathSupport Xpath()
    {
        return new XpathSupport();
    }
}
