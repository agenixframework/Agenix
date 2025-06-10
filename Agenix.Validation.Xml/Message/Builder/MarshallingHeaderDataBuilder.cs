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
using Agenix.Api.Xml;
using Agenix.Core.Message.Builder;

namespace Agenix.Validation.Xml.Message.Builder;

/// <summary>
/// Provides functionality for building message header data with
/// support for object marshalling. This class extends the capabilities of
/// <see cref="DefaultHeaderDataBuilder"/> by integrating marshaller-specific functionality.
/// </summary>
public class MarshallingHeaderDataBuilder : DefaultHeaderDataBuilder
{
 private readonly IMarshaller _marshaller;
    private readonly string _marshallerName;

    /// <summary>
    /// Default constructor using just model object.
    /// </summary>
    /// <param name="model">The model object</param>
    public MarshallingHeaderDataBuilder(object model) : base(model)
    {
        _marshaller = null;
        _marshallerName = null;
    }

    /// <summary>
    /// Default constructor using object marshaller and model object.
    /// </summary>
    /// <param name="model">The model object</param>
    /// <param name="marshaller">The marshaller instance</param>
    public MarshallingHeaderDataBuilder(object model, IMarshaller marshaller) : base(model)
    {
        _marshaller = marshaller;
        _marshallerName = null;
    }

    /// <summary>
    /// Default constructor using object marshaller name and model object.
    /// </summary>
    /// <param name="model">The model object</param>
    /// <param name="marshallerName">The marshaller name</param>
    public MarshallingHeaderDataBuilder(object model, string marshallerName) : base(model)
    {
        _marshallerName = marshallerName;
        _marshaller = null;
    }

    public override string BuildHeaderData(TestContext context)
    {
        if (HeaderData is null or string)
        {
            return base.BuildHeaderData(context);
        }

        if (_marshaller != null)
        {
            return BuildHeaderData(_marshaller, HeaderData, context);
        }

        if (_marshallerName != null)
        {
            if (context.ReferenceResolver.IsResolvable(_marshallerName))
            {
                var objectMapper = context.ReferenceResolver.Resolve<IMarshaller>(_marshallerName);
                return BuildHeaderData(objectMapper, HeaderData, context);
            }

            throw new AgenixSystemException($"Unable to find proper object marshaller for name '{_marshallerName}'");
        }

        var marshallerMap = context.ReferenceResolver.ResolveAll<IMarshaller>();
        if (marshallerMap.Count == 1)
        {
            return BuildHeaderData(marshallerMap.Values.First(), HeaderData, context);
        }

        throw new AgenixSystemException($"Unable to auto detect object marshaller - " +
                                        $"found {marshallerMap.Count} matching marshaller instances in reference resolver");
    }

    private static string BuildHeaderData(IMarshaller marshaller, object model, TestContext context)
    {
        try
        {
            using var stringWriter = new StringWriter();
            marshaller.Marshal(model, stringWriter);
            var result = stringWriter.ToString();

            return context.ReplaceDynamicContentInString(result);
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Failed to marshal object graph for message header data", e);
        }
    }

}
