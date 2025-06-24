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
using Agenix.Api.Spi;
using log4net;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Context;

public class AgenixContextTest
{
    [Test]
    public void ShouldParseConfiguration()
    {
        var context = AgenixContext.Create();
        context.ParseConfiguration(typeof(FooConfig));

        ClassicAssert.IsTrue(context.ReferenceResolver.IsResolvable("Foo"));
        ClassicAssert.AreEqual("Foo", context.ReferenceResolver.Resolve("Foo"));
        ClassicAssert.IsTrue(context.ReferenceResolver.IsResolvable("_bar"));
        ClassicAssert.AreEqual("Bar", context.ReferenceResolver.Resolve("_bar"));
        ClassicAssert.IsTrue(context.ReferenceResolver.IsResolvable("foobar"));
        ClassicAssert.AreEqual("FooBar", context.ReferenceResolver.Resolve("foobar"));
    }

    [Test]
    public void ShouldNoRaiseErrorWithPrivateMethod()
    {
        var context = AgenixContext.Create();
        context.ParseConfiguration(typeof(PrivateMethodConfig));

        ClassicAssert.IsTrue(context.ReferenceResolver.IsResolvable("Error"));
        ClassicAssert.AreEqual("should no fail", context.ReferenceResolver.Resolve("Error"));
    }

    [Test]
    public void ShouldRaiseErrorWithNoDefaultConstructor()
    {
        var context = AgenixContext.Create();

        var ex = Assert.Throws<AgenixSystemException>(() =>
            context.ParseConfiguration(typeof(NoDefaultConstructorConfig)));

        StringAssert.IsMatch("Missing default constructor on custom configuration class", ex.Message);
    }

    /// <summary>
    ///     Represents a configuration class containing private methods for use within the Agenix context.
    ///     This class is a demonstration of how private methods in a configuration class may interact with the context's
    ///     parsing mechanism.
    /// </summary>
    public class PrivateMethodConfig
    {
        [BindToRegistry]
        private string Error()
        {
            return "should no fail";
        }
    }

    /// <summary>
    ///     Represents a configuration class for the Agenix context that requires a non-default constructor.
    ///     This class demonstrates how configuration settings can be parsed and utilized within the context.
    /// </summary>
    public class NoDefaultConstructorConfig
    {
        public NoDefaultConstructorConfig(string config)
        {
            LogManager.GetLogger(typeof(NoDefaultConstructorConfig)).Info(config);
        }
    }

    /// <summary>
    ///     Represents a configuration class for the Agenix context used to define specific settings and registry bindings.
    ///     This class provides methods that return configuration values, which are registered in the context's reference
    ///     registry.
    /// </summary>
    private class FooConfig
    {
#pragma warning disable CS0414 // Field is assigned but its value is never used
        [BindToRegistry] private string _bar = "Bar";
#pragma warning restore CS0414 // Field is assigned but its value is never used

        [BindToRegistry]
        public string Foo()
        {
            return "Foo";
        }

        [BindToRegistry(Name = "foobar")]
        public string Bar()
        {
            return "FooBar";
        }
    }
}
