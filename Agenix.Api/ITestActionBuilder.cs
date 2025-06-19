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

using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Api;

/// <summary>
///     Test action builder.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ITestActionBuilder<out T> where T : ITestAction
{
    /// <summary>
    ///     Builds a new test action instance.
    /// </summary>
    /// <returns>the built test action.</returns>
    T Build();


    public interface IDelegatingTestActionBuilder<out TU> : ITestActionBuilder<TU> where TU : ITestAction
    {
        /// <summary>
        ///     Gets the delegate test action builder.
        /// </summary>
        ITestActionBuilder<TU> Delegate { get; }
    }

    /// <summary>
    /// Logger for TestActionBuilder operations
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(ITestActionBuilder<ITestAction>));

    /// <summary>
    /// Endpoint builder resource lookup path
    /// </summary>
    public const string ResourcePath = "Extension/agenix/action/builder";

    /// <summary>
    /// Default Agenix test action builders from classpath resource properties
    /// </summary>
    private static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

    /// <summary>
    /// Resolves all available test action builders from resource path lookup. Scans classpath for test action builder meta information
    /// and instantiates those builders.
    /// </summary>
    /// <returns>Dictionary of action builder name to builder instance</returns>
    public static IDictionary<string, ITestActionBuilder<ITestAction>> Lookup()
    {
        var builders = TypeResolver.ResolveAll<ITestActionBuilder<ITestAction>>();

        if (Logger.IsEnabled(LogLevel.Debug))
        {
            foreach (var (key, builder) in builders)
            {
                Logger.LogDebug("Found test action builder '{Key}' as {BuilderType}", key, builder.GetType());
            }
        }

        return builders;
    }

    /// <summary>
    /// Searches for available test action builders from the defined resource path.
    /// </summary>
    /// <returns>A dictionary containing the available test action builders, keyed by their names.</returns>
    public static Optional<ITestActionBuilder<ITestAction>> Lookup(string builder)
    {
        try
        {
            return Optional<ITestActionBuilder<ITestAction>>.Of(
                TypeResolver.Resolve<ITestActionBuilder<ITestAction>>(builder));
        }
        catch (AgenixSystemException ex)
        {
            Logger.LogWarning("Failed to resolve test action builder from resource '{ResourcePath}/{Builder}': {Error}",
                ResourcePath, builder, ex.Message);
        }

        return Optional<ITestActionBuilder<ITestAction>>.Empty;
    }
}
