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
using Agenix.Api.Container;
using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Api.Functions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Report;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Matcher;
using Agenix.Api.Variable;
using Agenix.Api.Xml.Namespace;
using Agenix.Core.Functions;
using Agenix.Core.Log;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Matcher;

namespace Agenix.Core;

/// <summary>
///     Factory class to create instances of TestContext with specified configurations.
/// </summary>
public class TestContextFactory : IReferenceResolverAware
{
    /// <summary>
    ///     Gets or sets the reference resolver.
    /// </summary>
    public IReferenceResolver _referenceResolver;

    /// <summary>
    ///     Gets or sets the function registry.
    /// </summary>
    public FunctionRegistry FunctionRegistry { get; set; }

    /// <summary>
    ///     Gets or sets the validation matcher registry.
    /// </summary>
    public ValidationMatcherRegistry ValidationMatcherRegistry { get; set; }

    /// <summary>
    ///     Gets or sets the log modifier.
    /// </summary>
    public ILogModifier LogModifier { get; set; }

    /// <summary>
    ///     Gets or sets the type converter.
    /// </summary>
    public ITypeConverter TypeConverter { get; set; }

    /// <summary>
    ///     Gets or sets the global variables used in each test case.
    /// </summary>
    public GlobalVariables GlobalVariables { get; set; } = new();

    /// <summary>
    ///     Gets or sets the test listeners responsible for spreading test events.
    /// </summary>
    public TestListeners TestListeners { get; set; }

    /// <summary>
    ///     Gets or sets the TestActionListeners instance responsible for broadcasting test action events
    ///     to all registered test action listeners.
    /// </summary>
    public TestActionListeners TestActionListeners { get; set; }

    /// <summary>
    ///     Gets or sets the collection of message listeners.
    /// </summary>
    public MessageListeners MessageListeners { get; set; }

    /// <summary>
    ///     Gets or sets the message processors, responsible for managing and notifying message listeners.
    /// </summary>
    public MessageProcessors MessageProcessors { get; set; }

    /// <summary>
    ///     Gets or sets the message validator registry.
    /// </summary>
    public MessageValidatorRegistry MessageValidatorRegistry { get; set; }

    /// <summary>
    ///     A list of actions to be executed prior to the test execution.
    /// </summary>
    public List<IBeforeTest> BeforeTest { get; set; } = [];

    /// <summary>
    ///     Collection of actions to be executed after a test completes.
    /// </summary>
    public List<IAfterTest> AfterTest { get; set; } = [];

    /// <summary>
    ///     Gets or sets the endpoint factory used for creating endpoint instances.
    /// </summary>
    public IEndpointFactory EndpointFactory { get; set; }

    /// <summary>
    ///     Gets or sets the registry for managing segment variable extractors.
    /// </summary>
    public SegmentVariableExtractorRegistry SegmentVariableExtractorRegistry { get; set; }

    /// <summary>
    ///     Gets the current reference resolver.
    /// </summary>
    /// <returns>The current instance of IReferenceResolver</returns>
    public IReferenceResolver ReferenceResolver => _referenceResolver;

    /// <summary>
    ///     Gets or sets the NamespaceContextBuilder, which facilitates the construction of namespace mappings
    ///     for XPath expression evaluations. This allows for both default and dynamic mappings derived
    ///     from received messages.
    /// </summary>
    public NamespaceContextBuilder NamespaceContextBuilder { get; set; }

    /// <summary>
    ///     Sets the reference resolver for the TestContextFactory.
    /// </summary>
    /// <param name="referenceResolver">
    ///     An instance of the IReferenceResolver interface used to resolve references.
    /// </param>
    public void SetReferenceResolver(IReferenceResolver referenceResolver)
    {
        _referenceResolver = referenceResolver;
    }

    /// <summary>
    ///     Gets a new instance of TestContext with a default/ core function library initialized.
    /// </summary>
    /// <returns>new instance of TestContext</returns>
    public TestContext GetObject()
    {
        var context = new TestContext
        {
            FunctionRegistry = FunctionRegistry,
            ValidationMatcherRegistry = ValidationMatcherRegistry,
            TestListeners = TestListeners,
            TestActionListeners = TestActionListeners,
            MessageListeners = MessageListeners,
            MessageValidatorRegistry = MessageValidatorRegistry,
            BeforeTest = BeforeTest,
            AfterTest = AfterTest,
            MessageProcessors = MessageProcessors,
            EndpointFactory = EndpointFactory,
            SegmentVariableExtractorRegistry = SegmentVariableExtractorRegistry
        };

        context.SetGlobalVariables(GlobalVariables);
        context.SetReferenceResolver(_referenceResolver);

        if (LogModifier != null)
        {
            context.LogModifier = LogModifier;
        }

        if (TypeConverter != null)
        {
            context.TypeConverter = TypeConverter;
        }

        if (NamespaceContextBuilder != null)
        {
            context.NamespaceContextBuilder = NamespaceContextBuilder;
        }

        return context;
    }

    /// <summary>
    ///     New instance of TestContextFactory
    /// </summary>
    /// <returns>an instance of TestContextFactory</returns>
    public static TestContextFactory NewInstance()
    {
        TestContextFactory factory = new()
        {
            FunctionRegistry = new DefaultFunctionRegistry(),
            ValidationMatcherRegistry = new DefaultValidationMatcherRegistry(),
            LogModifier = new DefaultLogModifier(),
            GlobalVariables = new GlobalVariables(),
            TypeConverter = ITypeConverter.LookupDefault(),
            TestListeners = new TestListeners(),
            TestActionListeners = new TestActionListeners(),
            MessageListeners = new MessageListeners(),
            _referenceResolver = new SimpleReferenceResolver(),
            MessageValidatorRegistry = new DefaultMessageValidatorRegistry(),
            MessageProcessors = new MessageProcessors(),
            EndpointFactory = new DefaultEndpointFactory(),
            SegmentVariableExtractorRegistry = new SegmentVariableExtractorRegistry(),
            NamespaceContextBuilder = new NamespaceContextBuilder()
        };

        return factory;
    }

    /// <summary>
    ///     Creates a copy of the given TestContext instance with all its properties and variables.
    /// </summary>
    /// <param name="context">The TestContext instance to be copied.</param>
    /// <returns>A new TestContext instance that is a copy of the provided context.</returns>
    public static TestContext CopyOf(TestContext context)
    {
        TestContext result = new()
        {
            FunctionRegistry = context.FunctionRegistry,
            ValidationMatcherRegistry = context.ValidationMatcherRegistry,
            LogModifier = context.LogModifier,
            MessageStore = context.MessageStore,
            TypeConverter = context.TypeConverter,
            TestListeners = context.TestListeners,
            MessageListeners = context.MessageListeners,
            MessageValidatorRegistry = context.MessageValidatorRegistry,
            MessageProcessors = context.MessageProcessors,
            EndpointFactory = context.EndpointFactory,
            NamespaceContextBuilder = context.NamespaceContextBuilder
        };

        foreach (var kvp in context.GetVariables())
        {
            result.GetVariables()[kvp.Key] = kvp.Value;
        }

        result.SetGlobalVariables(new GlobalVariables.Builder()
            .WithVariables(context.GetGlobalVariables())
            .Build());

        result.SetReferenceResolver(context.ReferenceResolver);

        return result;
    }
}
