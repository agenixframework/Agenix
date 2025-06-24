#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using System.Xml;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Api.Validation;
using Agenix.Api.Xml;
using Agenix.Core.Util;
using Agenix.Core.Validation;

namespace Agenix.Validation.Xml.Validation.Xml;

/// <summary>
///     Provides an abstract base class for XML marshaling validation processors.
///     Enables the unmarshalling of XML messages and facilitates validation logic by
///     integrating it with specific validation processes and contexts.
/// </summary>
/// <typeparam name="T">The type parameter representing the data type to be processed and validated.</typeparam>
public abstract class XmlMarshallingValidationProcessor<T> : AbstractValidationProcessor<T>
{
    private IUnmarshaller? _unmarshaller;

    protected XmlMarshallingValidationProcessor()
    {
    }

    /// Provides an abstract implementation for XML marshaling validation processors.
    /// Allows for unmarshalling XML data and integrating validation logic.
    /// /
    protected XmlMarshallingValidationProcessor(IUnmarshaller unmarshaller)
    {
        _unmarshaller = unmarshaller;
    }


    /// Unmarshal a message's payload to an object of type T using the configured unmarshaller.
    /// If the unmarshaller is not set, it attempts to resolve it from the reference resolver.
    /// Throws an exception if unmarshalling fails or if the configuration is incomplete.
    /// <param name="message">The message containing the payload to be unmarshalled.</param>
    /// <returns>An object of type T obtained by unmarshalling the message's payload.</returns>
    private T UnmarshalMessage(IMessage message)
    {
        if (_unmarshaller == null)
        {
            ObjectHelper.AssertNotNull(ReferenceResolver,
                "Marshalling validation callback requires marshaller instance " +
                "or proper reference resolver with nested bean definition of type marshaller");

            _unmarshaller = ReferenceResolver.Resolve<IUnmarshaller>();
        }

        try
        {
            return (T)_unmarshaller.Unmarshal(GetPayloadSource(message.Payload));
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Failed to unmarshal message payload", e);
        }
    }

    /// <summary>
    ///     Creates the payload source for unmarshalling.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    private static XmlReader GetPayloadSource(object? payload)
    {
        return payload switch
        {
            string stringPayload => XmlReader.Create(new StringReader(stringPayload)),
            FileInfo filePayload => XmlReader.Create(filePayload.FullName),
            XmlDocument documentPayload => XmlReader.Create(new StringReader(documentPayload.OuterXml)),
            XmlReader readerPayload => readerPayload,
            Stream streamPayload => XmlReader.Create(streamPayload),
            TextReader textReaderPayload => XmlReader.Create(textReaderPayload),
            _ => throw new AgenixSystemException("Failed to create payload source for unmarshalling message")
        };
    }

    /// Validates a given message within a specific context.
    /// Invokes the unmarshalling of the message, applies validation logic,
    /// and examines headers for validation consistency.
    /// <param name="message">The message to be validated implementing the <see cref="IMessage" /> interface.</param>
    /// <param name="context">The test context used for managing validation states and configurations.</param>
    public override void Validate(IMessage message, TestContext context)
    {
        Validate(UnmarshalMessage(message), message.GetHeaders(), context);
    }


    /// <summary>
    ///     Fluent builder.
    /// </summary>
    /// <typeparam name="B"></typeparam>
    public sealed class Builder<B>(GenericValidationProcessor<B> validationProcessor)
        : IMessageProcessor.IBuilder<XmlMarshallingValidationProcessor<B>, Builder<B>>, IReferenceResolverAware
    {
        private readonly GenericValidationProcessor<B>? _validationProcessor = validationProcessor;
        private IReferenceResolver? _referenceResolver;
        private IUnmarshaller? _unmarshaller;

        public XmlMarshallingValidationProcessor<B> Build()
        {
            if (_unmarshaller == null)
            {
                if (_referenceResolver != null)
                {
                    _unmarshaller = _referenceResolver.Resolve<IUnmarshaller>();
                }
                else
                {
                    throw new AgenixSystemException("Missing XML unmarshaller - " +
                                                    "please set proper unmarshaller or reference resolver");
                }
            }

            if (_validationProcessor == null)
            {
                throw new AgenixSystemException("Missing validation processor - " +
                                                "please add proper validation logic");
            }

            return new CustomXmlMarshallingValidationProcessor<B>(_unmarshaller, _validationProcessor);
        }

        public void SetReferenceResolver(IReferenceResolver newReferenceResolver)
        {
            _referenceResolver = newReferenceResolver;
        }

        public static Builder<T> Validate(GenericValidationProcessor<T> validationProcessor)
        {
            return new Builder<T>(validationProcessor);
        }

        public Builder<B> Unmarshaller(IUnmarshaller newUnmarshaller)
        {
            _unmarshaller = newUnmarshaller;
            return this;
        }

        public Builder<B> WithReferenceResolver(IReferenceResolver referenceResolver)
        {
            _referenceResolver = referenceResolver;
            return this;
        }

        // Private implementation class to handle the anonymous class behavior
        private class CustomXmlMarshallingValidationProcessor<TPayload>(
            IUnmarshaller unmarshaller,
            GenericValidationProcessor<TPayload> processor)
            : XmlMarshallingValidationProcessor<TPayload>(unmarshaller)
        {
            public override void Validate(TPayload payload, IDictionary<string, object> headers, TestContext context)
            {
                processor.Invoke(payload, headers, context);
            }
        }
    }
}
