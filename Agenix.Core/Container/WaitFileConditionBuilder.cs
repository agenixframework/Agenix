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
using Agenix.Core.Condition;

namespace Agenix.Core.Container;

/// A builder class for constructing wait conditions related to file operations.
/// This class provides methods to configure the conditions for waiting on file operations.
/// It extends the functionality of WaitConditionBuilder for handling instances of FileCondition.
public class WaitFileConditionBuilder : WaitConditionBuilder<FileCondition, WaitFileConditionBuilder>
{
    /// A builder class for constructing wait conditions related to file operations.
    /// Extends the {@link WaitConditionBuilder} for handling {@link FileCondition} instances.
    /// /
    public WaitFileConditionBuilder(Wait.Builder<FileCondition> builder) : base(builder)
    {
    }

    /// Wait for a given file path.
    /// <param name="filePath">The path of the file to wait for</param>
    /// <return>This instance of WaitFileConditionBuilder for method chaining</return>
    /// /
    public WaitFileConditionBuilder Path(string filePath)
    {
        GetCondition().SetFilePath(filePath);
        return self;
    }

    /// Wait for a given file resource.
    /// @param file The file resource to wait on
    /// @return This instance of WaitFileConditionBuilder for method chaining
    /// /
    public WaitFileConditionBuilder Resource(FileInfo file)
    {
        GetCondition().SetFile(file);
        return self;
    }
}
