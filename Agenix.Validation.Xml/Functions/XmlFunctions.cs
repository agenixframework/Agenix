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
using Agenix.Validation.Xml.Functions.Core;

namespace Agenix.Validation.Xml.Functions;

/// <summary>
///     Utility class providing static methods for XML-related functions.
/// </summary>
public static class XmlFunctions
{
    /// <summary>
    ///     Runs create CData section function with arguments.
    /// </summary>
    /// <param name="content">The content to wrap in CDATA</param>
    /// <param name="context">The test context</param>
    /// <returns>Content wrapped in CDATA section</returns>
    public static string CreateCDataSection(string content, TestContext context)
    {
        return new CreateCDataSectionFunction().Execute([content], context);
    }

    /// <summary>
    ///     Runs escape XML function with arguments.
    /// </summary>
    /// <param name="content">The content to escape</param>
    /// <param name="context">The test context</param>
    /// <returns>XML-escaped content</returns>
    public static string EscapeXml(string content, TestContext context)
    {
        return new EscapeXmlFunction().Execute([content], context);
    }

    /// <summary>
    ///     Runs XPath function with arguments.
    /// </summary>
    /// <param name="content">The XML content to evaluate</param>
    /// <param name="expression">The XPath expression</param>
    /// <param name="context">The test context</param>
    /// <returns>Result of XPath evaluation</returns>
    public static string XPath(string content, string expression, TestContext context)
    {
        return new XpathFunction().Execute([content, expression], context);
    }
}
