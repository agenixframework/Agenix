#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
