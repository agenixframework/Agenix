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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Agenix.Api.Log;

/// <summary>
///     Provides logging functionality through an abstraction layer for creating loggers associated with specific
///     categories or types.
/// </summary>
public static class LogManager
{
    /// <summary>
    ///     Gets or sets the current log provider based on the logger factory.
    /// </summary>
    public static ILoggerFactory? LoggerFactory { get; set; }

    /// <summary>
    ///     Retrieves a logger instance for the specified category name.
    /// </summary>
    /// <param name="category">The category name with which the logger is associated.</param>
    /// <returns>A logger instance for the specified category. If the LoggerFactory is null, a NullLogger instance is returned.</returns>
    public static ILogger GetLogger(string category)
    {
        return LoggerFactory?.CreateLogger(category) ?? NullLogger.Instance;
    }

    /// <summary>
    ///     Retrieves a logger instance for the specified type.
    /// </summary>
    /// <param name="type">The type with which the logger is associated.</param>
    /// <returns>A logger instance for the specified type. If the LoggerFactory is null, a NullLogger instance is returned.</returns>
    public static ILogger GetLogger(Type type)
    {
        return LoggerFactory?.CreateLogger(type) ?? NullLogger.Instance;
    }

    /// <summary>
    ///     Retrieves a logger instance for the specified type.
    /// </summary>
    /// <typeparam name="T">The type with which the logger is associated.</typeparam>
    /// <returns>A logger instance for the specified type. If the LoggerFactory is null, a NullLogger instance is returned.</returns>
    public static ILogger<T> GetLogger<T>()
    {
        return LoggerFactory?.CreateLogger<T>() ?? NullLogger<T>.Instance;
    }
}
