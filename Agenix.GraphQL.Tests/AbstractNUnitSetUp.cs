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
using Agenix.Validation.Xml.Validation.Xhtml;
using Agenix.Validation.Xml.Validation.Xml;
using NUnit.Framework.Interfaces;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.GraphQL.Tests;

public abstract class AbstractNUnitSetUp
{
     private TestContext _context;
    private TestContextFactory _testContextFactory;

    protected TestContext Context => _context;
    protected TestContextFactory TestContextFactory => _testContextFactory;

    [SetUp]
    public void Setup()
    {
        // Create completely isolated instances for each test
        _testContextFactory = CreateTestContextFactory();
        _context = CreateTestContext();
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            _context?.Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error during context cleanup: {ex.Message}");
        }
        finally
        {
            // Dispose resources safely
            if (_context is IDisposable disposableContext)
            {
                try { disposableContext.Dispose(); }
                catch (Exception ex) { Console.WriteLine($"Warning: Error disposing context: {ex.Message}"); }
            }

            if (_testContextFactory is IDisposable disposableFactory)
            {
                try { disposableFactory.Dispose(); }
                catch (Exception ex) { Console.WriteLine($"Warning: Error disposing factory: {ex.Message}"); }
            }

            _context = null;
            _testContextFactory = null;
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
        factory.MessageValidatorRegistry.AddMessageValidator("xml", new DomXmlMessageValidator());
        factory.MessageValidatorRegistry.AddMessageValidator("xpath", new XpathMessageValidator());
        factory.MessageValidatorRegistry.AddMessageValidator("xhtml", new XhtmlMessageValidator());
        factory.MessageValidatorRegistry.AddMessageValidator("xhtml-xpath", new XhtmlXpathMessageValidator());
    }

    protected virtual TestContext CreateTestContext()
    {
        try
        {
            return _testContextFactory.GetObject();
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Failed to create test context", e);
        }
    }

}
