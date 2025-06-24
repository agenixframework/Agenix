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

using System.Collections.Generic;
using Agenix.Api;
using Agenix.Api.Message;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Message;

/// <summary>
///     Test class for validating the functionality of the MessageStore within the Agenix framework.
/// </summary>
public class MessageStoreTest
{
    private readonly DirectEndpoint _directEndpoint = new DirectEndpointBuilder()
        .Queue(new DefaultMessageQueue("foo"))
        .Build();

    private TestContext _context;
    private ITestCaseRunner _t;

    [SetUp]
    public void SetupMethod()
    {
        _context = TestContextFactory.NewInstance().GetObject();
        _t = TestCaseRunnerFactory.CreateRunner(_context);

        _context.MessageValidatorRegistry.AddMessageValidator("simple", new SimpleMessageValidator());
    }

    [Test]
    public void ShouldStoreMessages()
    {
        _t.Run(Send()
            .Endpoint(_directEndpoint)
            .Message()
            .Name("request")
            .Body("Agenix rocks!"));

        ClassicAssert.NotNull(_context.MessageStore.GetMessage("request"));
        ClassicAssert.AreEqual("Agenix rocks!", _context.MessageStore.GetMessage("request").GetPayload<string>());

        _t.Run(Receive()
            .Endpoint(_directEndpoint)
            .Message()
            .Name("response")
            .Body("Agenix rocks!"));

        ClassicAssert.NotNull(_context.MessageStore.GetMessage("response"));
        ClassicAssert.AreEqual("Agenix rocks!", _context.MessageStore.GetMessage("response").GetPayload<string>());
    }

    [Test]
    public void ShouldStoreMessagesFromValidationCallback()
    {
        _t.Run(Send()
            .Endpoint(_directEndpoint)
            .Message()
            .Name("request")
            .Body("Agenix is awesome!"));

        ClassicAssert.NotNull(_context.MessageStore.GetMessage("request"));
        ClassicAssert.AreEqual("Agenix is awesome!", _context.MessageStore.GetMessage("request").GetPayload<string>());

        _t.Run(Receive()
            .Endpoint(_directEndpoint)
            .Message()
            .Name("response")
            .Validate((message, context) =>
            {
                var request = context.MessageStore.GetMessage("request");
                ClassicAssert.AreEqual(request.GetPayload<string>(), message.GetPayload<string>());
            }));


        ClassicAssert.NotNull(_context.MessageStore.GetMessage("response"));
        ClassicAssert.AreEqual("Agenix is awesome!", _context.MessageStore.GetMessage("response").GetPayload<string>());
    }

    private class SimpleMessageValidator : IMessageValidator<IValidationContext>
    {
        public void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
            List<IValidationContext> validationContexts)
        {
            ClassicAssert.AreEqual(receivedMessage.Payload, controlMessage.Payload);
            foreach (var ctx in validationContexts)
            {
                ctx.UpdateStatus(ValidationStatus.PASSED);
            }
        }

        public bool SupportsMessageType(string messageType, IMessage message)
        {
            return true;
        }
    }
}
