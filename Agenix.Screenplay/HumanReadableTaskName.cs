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

using System.Diagnostics;
using Agenix.Screenplay.Utils;

namespace Agenix.Screenplay;

/// <summary>
///     Provides functionality for generating a human-readable task name based on the currently executing method.
/// </summary>
public class HumanReadableTaskName
{
    /// Generates a human-readable task name for the currently executing method.
    /// <returns>
    ///     A string representing the human-readable name for the current method,
    ///     formatted by combining the class name and method name, and processed through the NameConverter.Humanize method.
    ///     The result excludes any methods defined within the "Agenix" namespace hierarchy.
    /// </returns>
    public static string ForCurrentMethod()
    {
        var stackTrace = new StackTrace();
        var businessIndex = 1;

        for (var methodIndex = 1; methodIndex < stackTrace.FrameCount; methodIndex++)
        {
            var frame = stackTrace.GetFrame(methodIndex);
            var method = frame?.GetMethod();
            if (method?.DeclaringType?.FullName?.StartsWith("Agenix") == true)
            {
                continue;
            }

            businessIndex = methodIndex;
            break;
        }

        var newFrame = stackTrace.GetFrame(businessIndex);
        var declaringType = newFrame?.GetMethod()?.DeclaringType;
        var className = declaringType?.FullName ?? "";
        var methodName = newFrame?.GetMethod()?.Name ?? "";

        var simpleClassName = className.Contains(".")
            ? className.Substring(className.LastIndexOf('.') + 1)
            : className;

        return NameConverter.Humanize($"{simpleClassName}_{methodName}");
    }
}
