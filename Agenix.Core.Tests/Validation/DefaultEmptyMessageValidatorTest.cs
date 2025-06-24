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

using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Validation;

/// <summary>
///     Unit tests for the DefaultEmptyMessageValidator class.
/// </summary>
public class DefaultEmptyMessageValidatorTest
{
    private Mock<IMessage> _controlMock;
    private Mock<IMessage> _receivedMock;
    private DefaultEmptyMessageValidator _validator;

    [SetUp]
    public void SetupMocks()
    {
        _receivedMock = new Mock<IMessage>();
        _controlMock = new Mock<IMessage>();
        _validator = new DefaultEmptyMessageValidator();
    }

    [Test]
    public void ShouldValidateEmptyMessage()
    {
        // Arrange
        _receivedMock.Setup(m => m.GetPayload<string>()).Returns(string.Empty);
        _controlMock.Setup(m => m.Payload).Returns(string.Empty);
        _controlMock.Setup(m => m.GetPayload<string>()).Returns(string.Empty);

        // Act and Assert
        Assert.DoesNotThrow(() =>
        {
            _validator.ValidateMessage(_receivedMock.Object, _controlMock.Object, new TestContext(),
                new DefaultValidationContext());
        });
    }

    [Test]
    public void ShouldSkipNullControlMessageMessage()
    {
        _validator.ValidateMessage(_receivedMock.Object, _controlMock.Object, new TestContext(),
            new DefaultValidationContext());
    }

    [Test]
    public void ShouldValidateNonEmptyMessage()
    {
        // Arrange
        _receivedMock.Setup(m => m.GetPayload<string>()).Returns("Hello");
        _controlMock.Setup(m => m.Payload).Returns(string.Empty);
        _controlMock.Setup(m => m.GetPayload<string>()).Returns(string.Empty);

        // Act and Assert
        var exception = Assert.Throws<ValidationException>(() =>
        {
            _validator.ValidateMessage(_receivedMock.Object, _controlMock.Object, new TestContext(),
                new DefaultValidationContext());
        });

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Validation failed - received message content is not empty!"));
    }

    [Test]
    public void ShouldValidateInvalidControlMessage()
    {
        // Arrange
        _receivedMock.Setup(m => m.GetPayload<string>()).Returns(string.Empty);
        _controlMock.Setup(m => m.Payload).Returns("Hello");
        _controlMock.Setup(m => m.GetPayload<string>()).Returns("Hello");

        // Act and Assert
        var exception = Assert.Throws<ValidationException>(() =>
        {
            _validator.ValidateMessage(_receivedMock.Object, _controlMock.Object, new TestContext(),
                new DefaultValidationContext());
        });

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Empty message validation failed - control message is not empty!"));
    }
}
