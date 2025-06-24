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
using System.IO;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Core.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Condition;

/// <summary>
///     Tests for the presence of a file and returns true if the file exists
/// </summary>
public class FileCondition() : AbstractCondition("file-check")
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(FileCondition));

    private FileInfo _file;

    /**
     * File path to check for existence
     */
    private string _filePath;

    /// Determines whether the file condition is satisfied by checking for the presence of the specified file.
    /// <param name="context">The test context providing necessary information for file path processing.</param>
    /// <return>True if the file exists and is not a directory; otherwise, false.</return>
    public override bool IsSatisfied(TestContext context)
    {
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug($"Checking file path '{(_file != null ? _file.FullName : _filePath)}'");
        }

        if (_file != null)
        {
            return _file.Exists && !_file.Attributes.HasFlag(FileAttributes.Directory);
        }

        try
        {
            return FileUtils.GetFileResource(context.ReplaceDynamicContentInString(_filePath), context).Exists;
        }
        catch (Exception e)
        {
            Log.LogWarning($"Failed to access file resource '{e.Message}'");
            return false;
        }
    }

    /// Retrieves the success message to indicate that the file condition has been met.
    /// <param name="context">The test context used to replace dynamic content in the file path.</param>
    /// <return>The success message confirming the existence of the file.</return>
    public override string GetSuccessMessage(TestContext context)
    {
        return
            $"File condition success - file '{(_file != null ? _file.FullName : context.ReplaceDynamicContentInString(_filePath))}' does exist";
    }

    /// Retrieves the error message when the file condition test fails.
    /// <param name="context">The test context used to replace dynamic content in the file path.</param>
    /// <return>The error message indicating the failure of the file condition.</return>
    public override string GetErrorMessage(TestContext context)
    {
        return
            $"Failed to check file condition - file '{(_file != null ? _file.FullName : context.ReplaceDynamicContentInString(_filePath))}' does not exist";
    }

    /// Retrieves the file path that is being checked for existence.
    /// @return The file path to be checked.
    /// /
    public string GetFilePath()
    {
        return _filePath;
    }

    /// Sets the filePath.
    /// <param name="filePath">The path to set</param>
    /// /
    public void SetFilePath(string filePath)
    {
        _filePath = filePath;
    }

    /// Gets the file.
    /// @return The file
    /// /
    public FileInfo GetFile()
    {
        return _file;
    }

    /// Sets the file.
    /// @param file The file to set
    /// /
    public void SetFile(FileInfo file)
    {
        _file = file;
    }

    public override string ToString()
    {
        return $"FileCondition{{filePath='{_filePath}', file={_file}, name={GetName()}}}";
    }
}
