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
using System.Collections.Generic;
using Agenix.Api.Message;
using Agenix.Core.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using static Agenix.Api.Validation.DefaultTextEqualsMessageValidator;

namespace Agenix.Core.Tests.Message;

/// <summary>
///     The DefaultMessageTest class contains unit tests for the DefaultMessage class using NUnit.
/// </summary>
public class DefaultMessageTest : AbstractNUnitSetUp
{
    [Test]
    public void TestPrint()
    {
        IMessage message = new DefaultMessage("<credentials><password>foo</password></credentials>");

        message.SetHeader("operation", "getCredentials");
        message.SetHeader("password", "foo");

        var output = message.Print();
        var expectedOutput = string.Format("IMESSAGE [" +
                                           "id: {0}, " +
                                           "payload: <credentials>\r\n  <password>foo</password>\r\n</credentials>\r\n" +
                                           "][headers: {{" +
                                           "agenix_message_id={0}, agenix_message_timestamp={1}, operation=getCredentials, password=foo" +
                                           "}}]", message.Id, message.GetTimestamp());

        ClassicAssert.AreEqual(NormalizeLineEndings(expectedOutput), NormalizeLineEndings(output));
    }

    [Test]
    public void TestPrintMaskKeyValue()
    {
        IMessage message = new DefaultMessage("password=foo,secret=bar");

        message.SetHeader("operation", "getCredentials");
        message.SetHeader("password", "foo");
        message.SetHeader("secret", "bar");

        var output = message.Print(Context);

        var expectedOutput = string.Format(
            "IMESSAGE [id: {0}, payload: password=****,secret=****][headers: {{agenix_message_id={0}, agenix_message_timestamp={1}, operation=getCredentials, password=****, secret=****}}]",
            message.Id, message.GetTimestamp());

        ClassicAssert.AreEqual(expectedOutput, output);
    }

    [Test]
    public void TestPrintMaskFormUrlEncoded()
    {
        IMessage message = new DefaultMessage("password=foo&secret=bar");

        message.SetHeader("operation", "getCredentials");
        message.SetHeader("password", "foo");
        message.SetHeader("secret", "bar");

        var output = message.Print(Context);

        var expectedOutput = string.Format(
            "IMESSAGE [id: {0}, payload: password=****&secret=****][headers: {{agenix_message_id={0}, agenix_message_timestamp={1}, operation=getCredentials, password=****, secret=****}}]",
            message.Id, message.GetTimestamp());

        ClassicAssert.AreEqual(expectedOutput, output);
    }

    [Test]
    public void TestPrintMaskXml()
    {
        IMessage message = new DefaultMessage("<credentials><password>foo</password></credentials>");

        message.SetHeader("operation", "getCredentials");
        message.SetHeader("password", "foo");

        var output = message.Print(Context);

        var expectedOutput = string.Format(
            "IMESSAGE [id: {0}, payload: <credentials>\r\n  <password>****</password>\r\n</credentials>\r\n][headers: {{agenix_message_id={0}, agenix_message_timestamp={1}, operation=getCredentials, password=****}}]",
            message.Id, message.GetTimestamp());

        ClassicAssert.AreEqual(NormalizeLineEndings(expectedOutput), NormalizeLineEndings(output));
    }

    [Test]
    public void TestPrintMaskJson()
    {
        IMessage message = new DefaultMessage("{ \"credentials\": { \"password\": \"foo\", \"secretKey\": \"bar\" }}");

        message.SetHeader("operation", "getCredentials");
        message.SetHeader("password", "foo");
        message.SetHeader("secretKey", "bar");

        var output = message.Print(Context);

        var expectedOutput = string.Format(
            "IMESSAGE [id: {0}, payload: {{\r\n  \"credentials\": {{\r\n    \"password\": \"****\",\r\n    \"secretKey\": \"****\"\r\n  }}\r\n}}][headers: {{agenix_message_id={0}, agenix_message_timestamp={1}, operation=getCredentials, password=****, secretKey=****}}]",
            message.Id, message.GetTimestamp());

        ClassicAssert.AreEqual(NormalizeLineEndings(expectedOutput), NormalizeLineEndings(output));
    }

    [Test]
    public void TestCopyConstructorPreservesIdAndTimestamp()
    {
        // Given
        var originalMessage = new DefaultMessage("myPayload", new Dictionary<string, object> { { "k1", "v1" } });

        // When
        var copiedMessage = new DefaultMessage(originalMessage);

        // Then
        ClassicAssert.AreEqual(originalMessage.GetHeader(MessageHeaders.Id),
            copiedMessage.GetHeader(MessageHeaders.Id));
        ClassicAssert.AreEqual(originalMessage.GetHeader(MessageHeaders.Timestamp),
            copiedMessage.GetHeader(MessageHeaders.Timestamp));
        ClassicAssert.AreEqual(originalMessage.GetHeader("k1"), copiedMessage.GetHeader("k1"));
        ClassicAssert.AreEqual(originalMessage.Payload, copiedMessage.Payload);
    }

    [Test]
    public void TestCopyConstructorWithAgenixOverwriteDoesNotPreserveIdAndTimestamp()
    {
        // Given
        var originalMessage = new DefaultMessage("myPayload", new Dictionary<string, object> { { "k1", "v1" } });
        originalMessage.SetHeader(MessageHeaders.Timestamp, DateTime.UtcNow.AddMilliseconds(-1));

        // When
        var copiedMessage = new DefaultMessage(originalMessage, true);

        // Then
        ClassicAssert.AreNotEqual(originalMessage.GetHeader(MessageHeaders.Id),
            copiedMessage.GetHeader(MessageHeaders.Id));
        ClassicAssert.AreNotEqual(originalMessage.GetHeader(MessageHeaders.Timestamp),
            copiedMessage.GetHeader(MessageHeaders.Timestamp));
        ClassicAssert.AreEqual(originalMessage.GetHeader("k1"), copiedMessage.GetHeader("k1"));
        ClassicAssert.AreEqual(originalMessage.Payload, copiedMessage.Payload);
    }
}
