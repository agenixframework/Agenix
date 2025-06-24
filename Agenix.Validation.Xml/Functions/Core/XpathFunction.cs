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

using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using Agenix.Api.Xml.Namespace;
using Agenix.Validation.Xml.Util;
using Agenix.Validation.Xml.Xpath;

namespace Agenix.Validation.Xml.Functions.Core;

/// <summary>
///     XPath function for evaluating XPath expressions against XML sources.
/// </summary>
public class XpathFunction : IFunction
{
    public string Execute(List<string> parameterList, TestContext context)
    {
        if (parameterList == null || parameterList.Count == 0)
        {
            throw new InvalidFunctionUsageException("Function parameters must not be empty");
        }

        if (parameterList.Count < 2)
        {
            throw new InvalidFunctionUsageException(
                "Missing parameter for function - usage xpath('xmlSource', 'expression')");
        }

        var xmlSource = parameterList[0];
        var xpathExpression = parameterList[1];

        var namespaceContext = new DefaultNamespaceContext();
        namespaceContext.AddNamespaces(context.NamespaceContextBuilder.NamespaceMappings);

        return XpathUtils.EvaluateAsString(
            XmlUtils.ParseMessagePayload(context.ReplaceDynamicContentInString(xmlSource)),
            context.ReplaceDynamicContentInString(xpathExpression),
            namespaceContext);
    }
}
