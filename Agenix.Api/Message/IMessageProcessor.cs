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

using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Message;

public delegate void MessageProcessor(IMessage message, TestContext context);

/// <summary>
///     Defines the contract for processing messages within the given context.
/// </summary>
public interface IMessageProcessor : IMessageTransformer
{
    /// <summary>
    ///     Represents the path used to locate resources for the message processor.
    /// </summary>
    private const string ResourcePath = "Extension/agenix/message/processor";

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IMessageProcessor));

    /// <summary>
    ///     Type resolver used to locate and retrieve custom message processors by performing
    ///     resource path lookups, enabling dynamic discovery and management of processor types.
    /// </summary>
    private static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

    /// <summary>
    ///     Processes the given message payload within the specified test context.
    /// </summary>
    /// <param name="message">The message to process.</param>
    /// <param name="context">The context in which the message is being processed.</param>
    void Process(IMessage message, TestContext context);

    /// <summary>
    ///     Transforms the given message within the specified test context.
    /// </summary>
    /// <param name="message">The message to transform.</param>
    /// <param name="context">The context in which the message is being transformed.</param>
    /// <returns>The transformed message.</returns>
    new IMessage Transform(IMessage message, TestContext context)
    {
        Process(message, context);
        return message;
    }

    /// <summary>
    ///     Retrieves an instance of the specified message processor builder based on the provided validator.
    /// </summary>
    /// <param name="processor">The identifier of the message processor builder to retrieve.</param>
    /// <typeparam name="T">The type of IMessageProcessor.</typeparam>
    /// <typeparam name="TB">The type of the builder for the IMessageProcessor.</typeparam>
    /// <returns>An instance of the message processor builder, or null if it fails to resolve.</returns>
    public static Optional<IBuilder<T, TB>> Lookup<T, TB>(string processor)
        where T : IMessageProcessor where TB : IBuilder<T, TB>
    {
        try
        {
            return Optional<IBuilder<T, TB>>.Of(TypeResolver.Resolve<TB>(processor));
        }
        catch (AgenixSystemException)
        {
            Log.LogWarning(
                "Failed to resolve message processor from resource '{ExtensionAgenixMessageProcessor}/{Processor}'",
                ResourcePath, processor);
        }

        return Optional<IBuilder<T, TB>>.Empty;
    }

    /// <summary>
    ///     Interface IBuilder defines the contract for building instances of
    ///     IMessageProcessor implementations.
    /// </summary>
    new interface IBuilder<out T, TB> : IMessageTransformer.IBuilder<T, TB>, IBuilder
        where T : IMessageProcessor
        where TB : IBuilder
    {
    }
}
