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

using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Api.Util;

namespace Agenix.Core.Message.Builder;

/// <summary>
///     Provides functionality to build and manage message header data.
/// </summary>
public class DefaultHeaderDataBuilder : IMessageHeaderDataBuilder
{
    /// Default constructor for initializing the header data.
    /// @param headerData The data representing the header fragment.
    /// /
    public DefaultHeaderDataBuilder(object headerData)
    {
        HeaderData = headerData;
    }

    /// Retrieves the header data.
    /// @return The header data object.
    /// /
    public object HeaderData { get; }

    /// Builds header data by replacing dynamic content in the header data string.
    /// @param context The context used to replace dynamic content in the header data string.
    /// @return A string with dynamic content replaced, or an empty string if header data is null.
    /// /
    public virtual string BuildHeaderData(TestContext context)
    {
        return HeaderData == null
            ? ""
            : context.ReplaceDynamicContentInString(HeaderData is string
                ? HeaderData.ToString()
                : TypeConversionUtils.ConvertIfNecessary<string>(HeaderData, typeof(string)));
    }

    public Dictionary<string, object> BuilderHeaders(TestContext context)
    {
        return new Dictionary<string, object>();
    }
}
