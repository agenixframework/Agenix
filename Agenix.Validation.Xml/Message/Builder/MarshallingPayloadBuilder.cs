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


using System.Text;
using System.Xml;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Xml;
using Agenix.Core.Message.Builder;

namespace Agenix.Validation.Xml.Message.Builder;

/// <summary>
/// Represents a payload builder for marshaling a model into a specific message format.
/// </summary>
/// <remarks>
/// This class supports the use of either custom implementations of marshallers or named marshallers for transforming model data into the required format.
/// It extends <see cref="DefaultPayloadBuilder"/> to provide additional functionality specific to marshalling operations.
/// </remarks>
[MessagePayload(MessageType.XML)]
public class MarshallingPayloadBuilder : DefaultPayloadBuilder
{
private readonly IMarshaller _marshaller;
    private readonly string _marshallerName;

    /// <summary>
    /// Default constructor using just a model object.
    /// </summary>
    /// <param name="model">The model object</param>
    public MarshallingPayloadBuilder(object model) : base(model)
    {
        _marshaller = null;
        _marshallerName = null;
    }

    /// <summary>
    /// Default constructor using object marshaler and model object.
    /// </summary>
    /// <param name="model">The model object</param>
    /// <param name="marshaller">The marshaler instance</param>
    public MarshallingPayloadBuilder(object model, IMarshaller marshaller) : base(model)
    {
        _marshaller = marshaller;
        _marshallerName = null;
    }

    /// <summary>
    /// Default constructor using object marshaller name and model object.
    /// </summary>
    /// <param name="model">The model object</param>
    /// <param name="marshallerName">The marshaller name</param>
    public MarshallingPayloadBuilder(object model, string marshallerName) : base(model)
    {
        _marshallerName = marshallerName;
        _marshaller = null;
    }

    public override object BuildPayload(TestContext context)
    {
        if (Payload == null || Payload is string)
        {
            return base.BuildPayload(context);
        }

        if (_marshaller != null)
        {
            return BuildPayload(_marshaller, Payload, context);
        }

        if (_marshallerName != null)
        {
            if (context.ReferenceResolver.IsResolvable(_marshallerName))
            {
                var objectMapper = context.ReferenceResolver.Resolve<IMarshaller>(_marshallerName);
                return BuildPayload(objectMapper, Payload, context);
            }
            else
            {
                throw new AgenixSystemException($"Unable to find proper object marshaller for name '{_marshallerName}'");
            }
        }

        var marshallerMap = context.ReferenceResolver.ResolveAll<IMarshaller>();
        if (marshallerMap.Count == 1)
        {
            return BuildPayload(marshallerMap.Values.First(), Payload, context);
        }
        else
        {
            throw new AgenixSystemException($"Unable to auto detect object marshaller - " +
                                            $"found {marshallerMap.Count} matching marshaller instances in reference resolver");
        }
    }

    /// <summary>
    /// Constructs the payload by marshaling the specified model into an XML format using the provided marshaller and context.
    /// </summary>
    /// <param name="marshaller">The marshaller responsible for serializing the model into XML.</param>
    /// <param name="model">The model object to be serialized into XML format.</param>
    /// <param name="context">The test context used for processing and replacing dynamic content in the resulting payload.</param>
    /// <returns>The serialized XML payload as an object with dynamic content replaced based on the context.</returns>
    private object BuildPayload(IMarshaller marshaller, object model, TestContext context)
    {
        var stringWriter = new StringWriter();

        try
        {
            marshaller.Marshal(model, stringWriter);
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Failed to marshal object graph for message payload", e);
        }

        return context.ReplaceDynamicContentInString(stringWriter.ToString());
    }

    /// <summary>
    /// Provides functionality to build a customized <see cref="MarshallingPayloadBuilder"/> instance.
    /// </summary>
    /// <remarks>
    /// This builder supports configuration for marshalling models either with named marshallers or custom marshaller implementations.
    /// </remarks>
    public class Builder : IMessagePayloadBuilder.IBuilder<MarshallingPayloadBuilder, Builder>
    {
        private object _model;
        private IMarshaller _marshaller;
        private string _marshallerName;

        /// <summary>
        /// Creates and initializes a new instance of the <see cref="Builder"/> class,
        /// setting the model object for the marshalling payload.
        /// </summary>
        /// <param name="model">The model object to be marshalled.</param>
        /// <returns>A new instance of <see cref="Builder"/> configured with the specified model.</returns>
        public static Builder Marshal(object model)
        {
            var builder = new Builder { _model = model };
            return builder;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Builder"/> class and initializes it with a model object.
        /// </summary>
        /// <param name="model">The model object to be marshalled.</param>
        /// <returns>A new instance of the <see cref="Builder"/> class configured with the specified model.</returns>
        public static Builder Marshal(object model, string marshaller)
        {
            var builder = new Builder { _model = model, _marshallerName = marshaller };
            return builder;
        }

        /// <summary>
        /// Creates and initializes a new instance of the <see cref="Builder"/> class,
        /// setting the model object and custom marshaller for the marshalling payload.
        /// </summary>
        /// <param name="model">The model object to be marshalled.</param>
        /// <param name="marshaller">The custom marshaller implementation to use for marshalling.</param>
        /// <returns>A new instance of <see cref="Builder"/> configured with the specified model and marshaller.</returns>
        public static Builder Marshal(object model, IMarshaller marshaller)
        {
            var builder = new Builder { _model = model, _marshaller = marshaller };
            return builder;
        }

        /// <summary>
        /// Sets the marshaller to be used by the builder.
        /// </summary>
        /// <param name="marshallerName">The name of the marshaller to use.</param>
        /// <returns>The current instance of the <see cref="Builder"/> class for chaining additional configuration steps.</returns>
        public Builder Marshaller(string marshallerName)
        {
            _marshallerName = marshallerName;
            return this;
        }

        /// <summary>
        /// Responsible for marshalling objects into an XML-based payload format.
        /// </summary>
        /// <param name="model">The model object to be marshalled.</param>
        /// <returns>An instance of <see cref="Marshaller"/> initialized for payload construction.</returns>
        public Builder Marshaller(IMarshaller marshaller)
        {
            _marshaller = marshaller;
            return this;
        }

        public MarshallingPayloadBuilder Build()
        {
            if (_marshaller != null)
            {
                return new MarshallingPayloadBuilder(_model, _marshaller);
            }

            return _marshallerName != null ? new MarshallingPayloadBuilder(_model, _marshallerName) : new MarshallingPayloadBuilder(_model);
        }
    }

}
