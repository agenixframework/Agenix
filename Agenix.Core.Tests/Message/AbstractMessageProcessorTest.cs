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

using System;
using Agenix.Api.Message;
using Agenix.Core.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Message;

public class AbstractMessageProcessorTest : AbstractNUnitSetUp
{
    [Test]
    public void TestProcessMessage()
    {
        var processor = new MockMessageProcessor();

        var inMessage = new DefaultMessage("Hello Agenix!");
        inMessage.SetType(MessageType.XML.ToString());
        processor.Process(inMessage, Context);
        ClassicAssert.AreEqual("Processed!", inMessage.GetPayload<string>());

        inMessage = new DefaultMessage("Hello Agenix!");
        inMessage.SetType(MessageType.PLAINTEXT.ToString());
        processor.Process(inMessage, Context);
        ClassicAssert.AreEqual("Hello Agenix!", inMessage.GetPayload<string>());
    }

    private class MockMessageProcessor : AbstractMessageProcessor
    {
        public override bool SupportsMessageType(string messageType)
        {
            return string.Equals(messageType, MessageType.XML.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public override void ProcessMessage(IMessage message, TestContext context)
        {
            message.Payload = "Processed!";
        }

        protected override string GetName()
        {
            return "MockProcessor";
        }
    }
}
