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
using System.IO;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using Agenix.Core.Util;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function reads a file from a given file path and returns the complete file content as a function result.
///     File content is automatically parsed for test variables.
/// </summary>
/// <remarks>
///     <para>File path can also have test variables as part of the file name or path.</para>
///     <para>The function accepts the following parameters:</para>
///     <list type="number">
///         <item>
///             <description>File path of the file resource to read</description>
///         </item>
///         <item>
///             <description>Boolean value to indicate that the returned value should be base64 encoded. Defaults to false.</description>
///         </item>
///         <item>
///             <description>
///                 Boolean value to indicate that a dynamic replacement should be performed before the content is
///                 base64 encoded. Defaults to false.
///             </description>
///         </item>
///     </list>
/// </remarks>
public class ReadFileResourceFunction : IFunction
{
    /// <summary>
    ///     Executes the read file resource function.
    /// </summary>
    /// <param name="parameterList">List of parameters containing file path and optional flags</param>
    /// <param name="testContext">Test context</param>
    /// <returns>File content as string, optionally base64 encoded</returns>
    /// <exception cref="InvalidFunctionUsageException">Thrown when parameters are null or empty</exception>
    /// <exception cref="AgenixSystemException">Thrown when file reading fails</exception>
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count == 0)
        {
            throw new InvalidFunctionUsageException("Missing file path function parameter");
        }

        var base64 = parameterList.Count > 1 && bool.Parse(parameterList[1]);

        try
        {
            if (base64)
            {
                if (parameterList.Count > 2 && bool.Parse(parameterList[2]))
                {
                    var content = ReadFileContent(parameterList[0], testContext, true);
                    var encoding = FileUtils.GetCharset(parameterList[0]);
                    return Convert.ToBase64String(encoding.GetBytes(content));
                }

                var fileBytes = FileUtils.CopyToByteArray(FileUtils.GetFileResource(parameterList[0], testContext));
                return Convert.ToBase64String(fileBytes);
            }

            return ReadFileContent(parameterList[0], testContext, true);
        }
        catch (IOException e)
        {
            throw new AgenixSystemException("Failed to read file", e);
        }
    }

    /// <summary>
    ///     Read the file content replacing dynamic content in the file content
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="context">Test context</param>
    /// <param name="replace">Whether to replace dynamic content</param>
    /// <returns>File content as string</returns>
    /// <exception cref="IOException">Thrown when file reading fails</exception>
    private static string ReadFileContent(string filePath, TestContext context, bool replace)
    {
        var content =
            FileUtils.ReadToString(FileUtils.GetFileResource(filePath, context), FileUtils.GetCharset(filePath));
        return replace ? context.ReplaceDynamicContentInString(content) : content;
    }
}
