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
using System.IO;
using System.Text;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Core.Util;

namespace Agenix.Core.Message.Builder;

/// <summary>
///     Class responsible for building header data from a file resource.
/// </summary>
public class FileResourceHeaderDataBuilder : IMessageHeaderDataBuilder
{
    private readonly string _charsetName;
    private readonly string _resourcePath;

    /// <summary>
    ///     Constructor using a file resource path and default charset.
    /// </summary>
    /// <param name="resourcePath"></param>
    public FileResourceHeaderDataBuilder(string resourcePath)
    {
        _resourcePath = resourcePath;
        _charsetName = AgenixSettings.AgenixFileEncoding();
    }

    /// <summary>
    ///     Constructor using file resource path and charset.
    /// </summary>
    /// <param name="resourcePath"></param>
    /// <param name="charsetName"></param>
    public FileResourceHeaderDataBuilder(string resourcePath, string charsetName)
    {
        _resourcePath = resourcePath;
        _charsetName = charsetName;
    }

    /// <summary>
    ///     Builds header data by reading and processing the file content based on the provided context.
    /// </summary>
    /// <param name="context">The TestContext object that contains necessary information for processing.</param>
    /// <returns>A string representing the processed header data.</returns>
    public string BuildHeaderData(TestContext context)
    {
        try
        {
            return context.ReplaceDynamicContentInString(FileUtils.ReadToString(
                FileUtils.GetFileResource(_resourcePath, context),
                Encoding.GetEncoding(context.ResolveDynamicValue(_charsetName))));
        }
        catch (IOException e)
        {
            throw new AgenixSystemException("Failed to read message header data resource", e);
        }
    }

    public Dictionary<string, object> BuilderHeaders(TestContext context)
    {
        return new Dictionary<string, object>();
    }
}
