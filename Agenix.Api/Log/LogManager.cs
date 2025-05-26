using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Agenix.Api.Log;

/// <summary>
/// Provides logging functionality through an abstraction layer for creating loggers associated with specific categories or types.
/// </summary>
public static class LogManager
{
    /// <summary>
    /// Gets or sets the current log provider based on the logger factory.
    /// </summary>
    public static ILoggerFactory? LoggerFactory { get; set; }

    /// <summary>
    /// Retrieves a logger instance for the specified category name.
    /// </summary>
    /// <param name="category">The category name with which the logger is associated.</param>
    /// <returns>A logger instance for the specified category. If the LoggerFactory is null, a NullLogger instance is returned.</returns>
    public static ILogger GetLogger(string category) => LoggerFactory?.CreateLogger(category) ?? NullLogger.Instance;

    /// <summary>
    /// Retrieves a logger instance for the specified type.
    /// </summary>
    /// <param name="type">The type with which the logger is associated.</param>
    /// <returns>A logger instance for the specified type. If the LoggerFactory is null, a NullLogger instance is returned.</returns>
    public static ILogger GetLogger(Type type) => LoggerFactory?.CreateLogger(type) ?? NullLogger.Instance;

    /// <summary>
    /// Retrieves a logger instance for the specified type.
    /// </summary>
    /// <typeparam name="T">The type with which the logger is associated.</typeparam>
    /// <returns>A logger instance for the specified type. If the LoggerFactory is null, a NullLogger instance is returned.</returns>
    public static ILogger<T> GetLogger<T>() => LoggerFactory?.CreateLogger<T>() ?? NullLogger<T>.Instance;
}
