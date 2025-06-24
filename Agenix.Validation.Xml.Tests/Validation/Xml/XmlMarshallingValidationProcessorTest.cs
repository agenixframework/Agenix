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
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Api.Validation;
using Agenix.Api.Xml;
using Agenix.Core.Message;
using Agenix.Validation.Xml.Validation.Xml;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Validation.Xml;

public class XmlMarshallingValidationProcessorTest : AbstractNUnitSetUp
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(XmlMarshallingValidationProcessorTest));

    private readonly IUnmarshaller _unmarshaller = new XmlReaderUnmarshaller();


    private readonly GenericValidationProcessor<string> _validationProcessor = (payload, headers, context) =>
        Log.LogInformation("Validating message {Payload}", payload);

    [Test]
    public void BuilderWithoutUnmarshallerTest()
    {
        var build = XmlMarshallingValidationProcessor<string>.Builder<string>.Validate(_validationProcessor)
            .Unmarshaller(_unmarshaller)
            .Build();

        build.SetReferenceResolver(new SimpleReferenceResolver());

        Assert.That(() => build.Validate(new DefaultMessage { Payload = "hi" }, null), Throws.Nothing);
    }

    [Test]
    public void BuilderWithUnmarshallerTest()
    {
        var referenceResolver = new SimpleReferenceResolver();
        referenceResolver.Bind("anyName", _unmarshaller);

        var build = XmlMarshallingValidationProcessor<string>.Builder<string>.Validate(_validationProcessor)
            .WithReferenceResolver(referenceResolver)
            .Build();

        build.SetReferenceResolver(referenceResolver);

        Assert.That(() => build.Validate(new DefaultMessage { Payload = "buy" }, null), Throws.Nothing);
    }


    private class XmlReaderUnmarshaller : IUnmarshaller
    {
        public object Unmarshal(XmlReader xmlReader)
        {
            return xmlReader == null
                ? string.Empty
                :
                // Read the XML content as string
                xmlReader.ReadOuterXml();

            // Alternative: Read inner XML content only
            // return xmlReader.ReadInnerXml();

            // Alternative: Read entire document
            // var doc = new XmlDocument();
            // doc.Load(xmlReader);
            // return doc.OuterXml;
        }
    }
}
