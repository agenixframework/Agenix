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
using Agenix.Api.Endpoint;

namespace Agenix.Api.Message;

/// Interface defining methods for converting between external and internal message types.
/// It supports converting an internal message to an external message for outbound communication,
/// and converting an external message to an internal message for inbound processing.
/// @param
/// <I>
///     The type representing the external message for inbound conversion.
///     @param
///     <O>
///         The type representing the external message for outbound conversion.
///         @param
///         <C>
///             The type representing the endpoint configuration, which must implement IEndpointConfiguration.
///             /
public interface IMessageConverter<in I, O, in C> where C : IEndpointConfiguration
{
    /// Converts an internal message to an external message for outbound communication.
    /// @param internalMessage The internal message that needs to be converted to an external format.
    /// @param endpointConfiguration The endpoint configuration that provides necessary context for the conversion process.
    /// @param context The test context containing additional data or state information required during the conversion.
    /// @return The converted external message suitable for outbound communication.
    /// /
    O ConvertOutbound(IMessage internalMessage, C endpointConfiguration, TestContext context);

    /// Converts an internal message to an external message for outbound communication.
    /// <param name="externalMessage">
    ///     The external message object that will be enriched with information from the internal
    ///     message.
    /// </param>
    /// <param name="internalMessage">The internal message that provides data for the conversion process.</param>
    /// <param name="endpointConfiguration">The endpoint configuration that provides necessary context for the conversion.</param>
    /// <param name="context">The test context containing additional data or state information required during the conversion.</param>
    void ConvertOutbound(O externalMessage, IMessage internalMessage, C endpointConfiguration, TestContext context);

    /// Converts an external message to an internal representation for inbound processing.
    /// <param name="externalMessage">The external message that needs to be converted to an internal format.</param>
    /// <param name="endpointConfiguration">The endpoint configuration providing necessary context for the conversion process.</param>
    /// <param name="context">The test context containing additional data or state information required during the conversion.</param>
    /// <return>The converted internal message suitable for processing within the system.</return>
    IMessage ConvertInbound(I externalMessage, C endpointConfiguration, TestContext context);
}
