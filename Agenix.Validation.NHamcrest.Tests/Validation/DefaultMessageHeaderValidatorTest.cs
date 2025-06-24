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
using Agenix.Api.Validation.Context;
using Agenix.Core.Message;
using Agenix.Core.Validation;
using NHamcrest;

namespace Agenix.Validation.NHamcrest.Tests.Validation;

public class DefaultMessageHeaderValidatorTest : AbstractNUnitSetUp
{
    private readonly HeaderValidationContext _validationContext = new();
    private readonly DefaultMessageHeaderValidator _validator = new();

    [Test]
    public void TestValidateMessageHeadersHamcrestMatcherSupport()
    {
        var receivedMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", "foo_test")
            .SetHeader("additional", "additional")
            .SetHeader("bar", "bar_test");
        var controlMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", Starts.With("foo"))
            .SetHeader("bar", Ends.With("_test"));

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestValidateHamcrestMatcherError()
    {
        var receivedMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", "foo_test")
            .SetHeader("bar", "bar_test");
        var controlMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", Starts.With("bar"))
            .SetHeader("bar", Ends.With("_test"));

        Assert.Throws<ValidationException>(() =>
            _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext));
    }
}
