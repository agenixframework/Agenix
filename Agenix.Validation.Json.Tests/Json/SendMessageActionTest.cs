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

using Agenix.Api.Endpoint;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Api.Spi;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Builder;
using Agenix.Validation.Json.Validation;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Json.Tests.Json;

public class SendMessageActionTest : AbstractNUnitSetUp
{
    private readonly Mock<IEndpointConfiguration> _endpointConfigurationMock = new();
    private readonly Mock<IEndpoint> _endpointMock = new();
    private readonly Mock<IProducer> _producerMock = new();

    protected override TestContextFactory CreateTestContextFactory()
    {
        var factory = base.CreateTestContextFactory();
        factory.MessageValidatorRegistry = new MessageValidatorRegistry();
        return factory;
    }

    [Test]
    public void TestSendMessageOverwriteMessageElementsJsonPath()
    {
        // Setup message builder with placeholder
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("{ \"TestRequest\": { \"Message\": \"?\" }}"));

        // Set up JsonPath expressions to modify a message
        var overwriteElements = new Dictionary<string, object> { ["$.TestRequest.Message"] = "Hello World!" };

        var processor = new JsonPathMessageProcessor.Builder()
            .Expressions(overwriteElements)
            .Build();

        // Expected message after processing
        var controlMessage = new DefaultMessage(
            "{\"TestRequest\":{\"Message\":\"Hello World!\"}}");

        // Reset and setup mocks
        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(x => x.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(x => x.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);


        // Set up verification of a sent message
        _producerMock
            .Setup(x => x.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
                ValidateMessageToSend(message, controlMessage));

        // Create and execute the send action
        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(messageBuilder)
            .Type(MessageType.JSON)
            .Process(processor)
            .Build();

        sendAction.Execute(Context);
    }

    [Test]
    public void TestSendJsonMessageWithValidation()
    {
        // Setup validation flag
        var validated = false;

        // Setup schema validator mock
        var schemaValidator = new Mock<ISchemaValidator<ISchemaValidationContext>>();
        schemaValidator.Setup(x => x.SupportsMessageType("JSON", It.IsAny<IMessage>()))
            .Returns(true);

        schemaValidator.Setup(x => x.Validate(
                It.IsAny<IMessage>(),
                It.IsAny<TestContext>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback<IMessage, TestContext, string, string>((message, context, schemaRepo, schema) =>
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(schema, Is.EqualTo("fooSchema"));
                    Assert.That(schemaRepo, Is.EqualTo("fooRepository"));
                }

                validated = true;
            });

        schemaValidator.Setup(x => x.CanValidate(
                It.IsAny<IMessage>(),
                It.IsAny<bool>()))
            .Returns(true);

        // Setup reference resolver spy
        var referenceResolverSpy = new Mock<IReferenceResolver>();
        Context.SetReferenceResolver(referenceResolverSpy.Object);

        referenceResolverSpy
            .Setup(x => x.ResolveAll<ISchemaValidator<ISchemaValidationContext>>())
            .Returns(new Dictionary<string, ISchemaValidator<ISchemaValidationContext>>
            {
                ["jsonSchemaValidator"] = schemaValidator.Object
            });

        // Add schema validator to registry
        Context.MessageValidatorRegistry.AddSchemeValidator("JSON", schemaValidator.Object);

        // Setup message builder
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("{ \"TestRequest\": { \"Message\": \"?\" }}"));

        // Reset and setup mocks
        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(x => x.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(x => x.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        // Create and execute the send action with schema validation
        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(messageBuilder)
            .SchemaValidation(true)
            .Schema("fooSchema")
            .SchemaRepository("fooRepository")
            .Type(MessageType.JSON)
            .Build();

        sendAction.Execute(Context);

        // Verify validation was performed
        Assert.That(validated, Is.True);
    }

    private void ValidateMessageToSend(IMessage toSend, IMessage controlMessage)
    {
        // Verify payload
        Assert.That(toSend.GetPayload<string>().Trim(),
            Is.EqualTo(controlMessage.GetPayload<string>().Trim()));

        // Verify headers
        var validator = new DefaultMessageHeaderValidator();
        validator.ValidateMessage(
            toSend,
            controlMessage, Context,
            new HeaderValidationContext.Builder().Build());
    }
}
