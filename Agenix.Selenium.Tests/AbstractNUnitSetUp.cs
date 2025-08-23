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
using Agenix.Core;
using Agenix.Core.Validation;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Selenium.Tests;

public abstract class AbstractNUnitSetUp
{
    protected TestContext Context { get; private set; }

    protected TestContextFactory TestContextFactory { get; private set; }

    [SetUp]
    public void Setup()
    {
        // Create completely isolated instances for each test
        TestContextFactory = CreateTestContextFactory();
        Context = CreateTestContext();
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            Context?.Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error during context cleanup: {ex.Message}");
        }
        finally
        {
            // Dispose resources safely
            if (Context is IDisposable disposableContext)
            {
                try { disposableContext.Dispose(); }
                catch (Exception ex) { Console.WriteLine($"Warning: Error disposing context: {ex.Message}"); }
            }

            if (TestContextFactory is IDisposable disposableFactory)
            {
                try { disposableFactory.Dispose(); }
                catch (Exception ex) { Console.WriteLine($"Warning: Error disposing factory: {ex.Message}"); }
            }

            Context = null;
            TestContextFactory = null;
        }
    }

    protected virtual TestContextFactory CreateTestContextFactory()
    {
        var factory = TestContextFactory.NewInstance();
        ConfigureMessageValidators(factory);
        return factory;
    }

    protected virtual void ConfigureMessageValidators(TestContextFactory factory)
    {
        factory.MessageValidatorRegistry.AddMessageValidator("header", new DefaultMessageHeaderValidator());
    }

    protected virtual TestContext CreateTestContext()
    {
        try
        {
            return TestContextFactory.GetObject();
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Failed to create test context", e);
        }
    }
}
