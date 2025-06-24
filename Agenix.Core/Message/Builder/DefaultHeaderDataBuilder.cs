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

using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Api.Util;

namespace Agenix.Core.Message.Builder;

/// <summary>
///     Provides functionality to build and manage message header data.
/// </summary>
public class DefaultHeaderDataBuilder : IMessageHeaderDataBuilder
{
    /// Default constructor for initializing the header data.
    /// @param headerData The data representing the header fragment.
    /// /
    public DefaultHeaderDataBuilder(object headerData)
    {
        HeaderData = headerData;
    }

    /// Retrieves the header data.
    /// @return The header data object.
    /// /
    public object HeaderData { get; }

    /// Builds header data by replacing dynamic content in the header data string.
    /// @param context The context used to replace dynamic content in the header data string.
    /// @return A string with dynamic content replaced, or an empty string if header data is null.
    /// /
    public virtual string BuildHeaderData(TestContext context)
    {
        return HeaderData == null
            ? ""
            : context.ReplaceDynamicContentInString(HeaderData is string
                ? HeaderData.ToString()
                : TypeConversionUtils.ConvertIfNecessary<string>(HeaderData, typeof(string)));
    }

    public Dictionary<string, object> BuilderHeaders(TestContext context)
    {
        return new Dictionary<string, object>();
    }
}
