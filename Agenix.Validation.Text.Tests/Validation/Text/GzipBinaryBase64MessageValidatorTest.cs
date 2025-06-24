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

using System.IO.Compression;
using System.Text;
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Context;
using Agenix.Core.Message;
using Agenix.Validation.Text.Validation.Text;
using NUnit.Framework;

namespace Agenix.Validation.Text.Tests.Validation.Text;

public class GzipBinaryBase64MessageValidatorTest : AbstractNUnitSetUp
{
    private readonly IValidationContext _validationContext = new DefaultValidationContext();
    private readonly GzipBinaryBase64MessageValidator _validator = new();

    [Test]
    public void TestGzipBinaryBase64Validation()
    {
        var receivedMessage = new DefaultMessage(GetZippedContent("Hello World!"));
        var controlMessage = new DefaultMessage(Convert.ToBase64String("Hello World!"u8.ToArray()));

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestGzipBinaryBase64ValidationNoBinaryData()
    {
        var receivedMessage = new DefaultMessage("SGVsbG8gV29ybGQh");
        var controlMessage = new DefaultMessage(Convert.ToBase64String("Hello World!"u8.ToArray()));

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestBinaryBase64ValidationError()
    {
        var receivedMessage = new DefaultMessage(GetZippedContent("Hello World!"));
        var controlMessage = new DefaultMessage(Convert.ToBase64String("Hello Agenix!"u8.ToArray()));

        var exception = Assert.Throws<ValidationException>(() =>
            _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message.Contains("expected 'SGVsbG8gQWdlbml4IQ=='"), Is.True);
        Assert.That(exception.Message.Contains("but was 'SGVsbG8gV29ybGQh'"), Is.True);
    }

    /// <summary>
    ///     Compresses the given string payload using GZip and returns the compressed byte array.
    /// </summary>
    /// <param name="payload">The string payload to be compressed.</param>
    /// <returns>A byte array containing the compressed data.</returns>
    private static byte[] GetZippedContent(string payload)
    {
        using var zipped = new MemoryStream();
        using (var gzipOutputStream = new GZipStream(zipped, CompressionMode.Compress))
        {
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            gzipOutputStream.Write(payloadBytes, 0, payloadBytes.Length);
            gzipOutputStream.Flush();
            gzipOutputStream.Close();
        }

        return zipped.ToArray();
    }
}
