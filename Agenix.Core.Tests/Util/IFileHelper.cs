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

using System.IO;
using Agenix.Api.Exceptions;

namespace Agenix.Core.Tests.Util;

public interface IFileHelper
{
    /// <summary>
    ///     Creates a temporary file with a unique file name using the system's temporary folder,
    ///     and modifies the extension to ".agenix.tmp". Deletes the file immediately after creation.
    /// </summary>
    /// <returns>A FileInfo object representing the created temporary file.</returns>
    /// <exception cref="AgenixSystemException">Thrown if there is an I/O error during temporary file creation.</exception>
    public static FileInfo CreateTmpFile()
    {
        FileInfo tempFile;
        try
        {
            // Create the temporary file with .tmp extension
            var tempFilePath = Path.GetTempFileName();

            // Create the new path with .agenix.txt extension
            var newFilePath = Path.Combine(Path.GetDirectoryName(tempFilePath) ?? string.Empty,
                Path.GetFileNameWithoutExtension(tempFilePath) + "agenix.tmp");

            // Rename the file by moving it to the new path
            File.Move(tempFilePath, newFilePath);

            // Return the FileInfo of the new file
            tempFile = new FileInfo(newFilePath);
        }
        catch (IOException e)
        {
            throw new AgenixSystemException(e.Message, e);
        }

        return tempFile;
    }
}
