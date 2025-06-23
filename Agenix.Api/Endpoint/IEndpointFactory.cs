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

using Agenix.Api.Annotations;
using Agenix.Api.Context;

namespace Agenix.Api.Endpoint;

/// <summary>
///     Endpoint factory tries to get an endpoint instance by parsing an endpoint uri. Uri can have parameters that get
///     passed
///     to the endpoint configuration. If Spring application context is given searches for matching endpoint component bean
///     and delegates to a component for endpoint creation.
/// </summary>
public interface IEndpointFactory
{
    /// <summary>
    ///     Finds endpoint by parsing the given endpoint URI. The test context helps to create endpoints
    ///     by providing the reference resolver so registered beans and bean references can be set as
    ///     configuration properties.
    /// </summary>
    /// <param name="endpointUri">The endpoint URI.</param>
    /// <param name="context">The test context.</param>
    /// <returns>The created endpoint.</returns>
    IEndpoint Create(string endpointUri, TestContext context);

    /// <summary>
    ///     Finds endpoint by parsing the given endpoint annotation. The test context helps to create endpoints
    ///     by providing the reference resolver so registered beans and bean references can be set as
    ///     configuration properties.
    /// </summary>
    /// <param name="endpointName">The endpoint name.</param>
    /// <param name="endpointConfig">The endpoint configuration as an annotation.</param>
    /// <param name="context">The test context.</param>
    /// <returns>The created endpoint.</returns>
    IEndpoint Create(string endpointName, Attribute endpointConfig, TestContext context);

    /// <summary>
    ///     Finds endpoint by parsing the given endpoint properties. The test context helps to create endpoints
    ///     by providing the reference resolver so registered beans and bean references can be set as
    ///     configuration properties.
    /// </summary>
    /// <param name="endpointName">The endpoint name.</param>
    /// <param name="endpointConfig">The endpoint configuration as AgenixEndpoint.</param>
    /// <param name="endpointType">The type of the endpoint.</param>
    /// <param name="context">The test context.</param>
    /// <returns>The created endpoint.</returns>
    IEndpoint Create(string endpointName, AgenixEndpointAttribute endpointConfig, Type endpointType,
        TestContext context);
}
