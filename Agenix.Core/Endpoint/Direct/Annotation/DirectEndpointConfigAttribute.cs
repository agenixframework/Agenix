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

using System;
using Agenix.Api.Annotations;

namespace Agenix.Core.Endpoint.Direct.Annotation;

/// <summary>
///     The DirectEndpointConfigAttribute is used to configure direct endpoint settings for a specific field.
///     This attribute is non-inheritable and targets field declarations.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
[AgenixEndpointConfig("direct.async")]
public class DirectEndpointConfigAttribute : Attribute
{
    /// <summary>
    ///     Destination name.
    /// </summary>
    public string QueueName { get; set; } = "";

    /// <summary>
    ///     Destination reference.
    /// </summary>
    public string Queue { get; set; } = "";

    /// <summary>
    ///     Timeout.
    /// </summary>
    public long Timeout { get; set; } = 5000L;
}
