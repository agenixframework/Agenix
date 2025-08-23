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
using Agenix.Api;
using Agenix.Api.Common;
using Agenix.Api.Container;
using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Report;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Api.Validation.Matcher;
using Agenix.Api.Variable;
using Agenix.Api.Xml.Namespace;
using Agenix.Core.Annotations;
using Agenix.Core.Functions;
using Agenix.Core.Log;
using Agenix.Core.Report;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Matcher;

namespace Agenix.Core;

/// <summary>
///     Default Agenix context implementation holds basic components used in Agenix.
/// </summary>
public class AgenixContext : IAsyncTestListenerAware, IAsyncTestActionListenerAware, IAsyncTestSuiteListenerAware,
    IAsyncTestReporterAware,
    IAsyncMessageListenerAware, IReferenceRegistry
{
    /// <summary>
    ///     Represents the primary context for handling test execution within the Agenix framework,
    ///     providing access to test listeners, action listeners, suite listeners,
    ///     and other configuration and functional components necessary for test orchestration.
    /// </summary>
    protected AgenixContext(Builder builder)
    {
        SuiteListeners = builder._testSuiteListeners;
        TestListeners = builder._testListeners;
        TestActionListeners = builder._testActionListeners;
        TestReporters = builder._testReporters;

        BeforeSuites = builder._beforeSuite;
        AfterSuites = builder._afterSuite;

        FunctionRegistry = builder._functionRegistry;
        ValidationMatcherRegistry = builder._validationMatcherRegistry;
        GlobalVariables = builder._globalVariables;
        MessageValidatorRegistry = builder._messageValidatorRegistry;
        MessageListeners = builder._messageListeners;
        EndpointFactory = builder._endpointFactory;
        ReferenceResolver = builder._referenceResolver;
        MessageProcessors = builder._messageProcessors;
        TypeConverter = builder._typeConverter;
        LogModifier = builder._logModifier;
        NamespaceContextBuilder = builder._namespaceContextBuilder;

        ContextFactory = builder._testContextFactory;

        foreach (var config in builder._configurationClasses)
        {
            ParseConfiguration(config);
        }
    }

    /// Provides a mechanism for creating instances of `TestContext`.
    /// This property facilitates the production of new `TestContext` objects as needed during
    /// the execution of tests, ensuring that each test operates within its own isolated context.
    /// The `ContextFactory` integrates with the various registries and components of Agenix,
    /// allowing for seamless configuration and context-specific behavior across tests.
    public TestContextFactory ContextFactory { get; }

    /// Provides access to the collection of asynchronous test suite listeners.
    /// This property allows managing and interacting with a set of listeners that respond
    /// to lifecycle events during the execution of test suites. The `SuiteListeners` play a
    /// pivotal role in tracking and handling suite-level behavior across the testing framework.
    public AsyncTestSuiteListeners SuiteListeners { get; }

    /// Manages the collection of asynchronous test listeners associated with the current context.
    /// This property supports the addition, removal, and lifecycle management of listeners
    /// that react to various test-related events during the execution of tests.
    /// The `TestListeners` property enables custom behavior and extensibility for test frameworks
    /// by facilitating integration with asynchronous event handling mechanisms.
    public AsyncTestListeners TestListeners { get; }

    /// Manages a collection of asynchronous test action listeners in the Agenix framework.
    /// This property allows for the aggregation and coordination of multiple `IAsyncTestActionListener`
    /// implementations. These listeners are invoked during the execution of specific test actions,
    /// enabling custom behavior or processing to be added at different stages of a test's lifecycle.
    /// `TestActionListeners` supports the dynamic addition of new listeners and plays a
    /// critical role in extending and customizing test execution workflows.
    public AsyncTestActionListeners TestActionListeners { get; }

    /// Represents a collection of asynchronous test reporters used to handle and manage test reporting.
    /// This property allows for the aggregation and coordination of multiple test reporters, providing
    /// a centralized mechanism for capturing, processing, and delivering test results during and after test execution.
    /// The `TestReporters` interface ensures compatibility with various components in the Agenix framework
    /// to facilitate robust and flexible test result management workflows.
    public AsyncTestReporters TestReporters { get; }

    /// Represents a collection of actions to be executed before the start of all test suites.
    /// These actions, represented by `IBeforeSuite` instances, are designed to perform any
    /// necessary setup or initialization required prior to the execution of the test suites.
    /// The `BeforeSuites` property ensures that pre-suite logic is consistently
    /// applied across different test runs and scenarios.
    public List<IBeforeSuite> BeforeSuites { get; }

    /// A list of `IAfterSuite` instances executed after a test suite completes.
    /// This property holds a collection of actions that are triggered after the entire
    /// test suite has finished execution, allowing for finalization or cleanup operations.
    /// Each `IAfterSuite` instance determines whether it should execute based on the
    /// context of the test suite that just ran, such as the suite's name or the groups included.
    public List<IAfterSuite> AfterSuites { get; }

    /// Manages the registration and organization of function libraries used within the Agenix framework.
    /// This property serves as a centralized repository for custom and predefined functions,
    /// enabling dynamic function invocation and providing extensibility for user-defined logic
    /// during test executions.
    public FunctionRegistry FunctionRegistry { get; }

    /// Represents a registry for managing and organizing validation matchers used within the Agenix framework.
    /// This property centralizes the configuration and access to validation matchers, allowing a consistent
    /// approach to validation operations across tests and contexts.
    /// The `ValidationMatcherRegistry` is integral to the framework's validation subsystem, ensuring
    /// that all defined matchers are properly registered and available for use.
    public ValidationMatcherRegistry ValidationMatcherRegistry { get; }

    /// Represents a centralized repository of global variables within the Agenix context.
    /// This property provides a mechanism for managing and accessing shared state or data
    /// that is intended to be available across various components and tests in the system.
    /// It enables seamless integration of globally scoped variables with the core functionality
    /// of the test framework, allowing for consistent data access and manipulation.
    public GlobalVariables GlobalVariables { get; }

    /// Represents a registry for managing and maintaining message validators within the context.
    /// This property centralizes the creation and organization of validators, allowing for consistent
    /// and reusable validation logic across various parts of the system.
    /// The `MessageValidatorRegistry` is used to register, retrieve, and apply message validators,
    /// ensuring compliance and validation of messages in workflows or test scenarios.
    public MessageValidatorRegistry MessageValidatorRegistry { get; }

    /// Provides a collection of asynchronous message listeners to handle message-related events within the testing context.
    /// This property allows for the registration and management of message listeners, enabling the execution of
    /// custom behaviors or validations in response to specific messages processed during the test lifecycle.
    /// The `MessageListeners` property integrates seamlessly with other context components to facilitate consistent
    /// message handling and reporting throughout test execution.
    public AsyncMessageListeners MessageListeners { get; }

    /// Responsible for creating and managing endpoint instances within the Agenix framework.
    /// This property allows for dynamic instantiation and configuration of endpoints used during tests,
    /// enabling flexible integration with external systems or services. The `EndpointFactory` is an
    /// essential component for managing endpoint lifecycles and ensuring consistency in endpoint behavior.
    public IEndpointFactory EndpointFactory { get; }

    /// Provides a mechanism for resolving object references within the Agenix framework.
    /// This property enables the resolution of registered references during the execution of tests
    /// or other framework operations, facilitating dynamic retrieval of objects by name or identifier.
    /// The `ReferenceResolver` aligns with the `IReferenceRegistry` interface, allowing it to
    /// manage and query references efficiently, ensuring seamless integration within the Agenix context.
    public IReferenceResolver ReferenceResolver { get; }

    /// Represents a collection of message processors used for handling and processing messages within the test context.
    /// This property manages the registration, execution, and coordination of message processing tasks,
    /// supporting integration with other context components and ensuring extensibility for custom message handling.
    public MessageProcessors MessageProcessors { get; }

    /// Provides a mechanism for converting objects between different types within the Agenix framework.
    /// This property enables seamless transformation of data to support various operations such as validation,
    /// message processing, and context-specific behavior.
    /// The `TypeConverter` integrates with other components to ensure that type conversions are handled
    /// consistently and efficiently throughout the framework.
    public ITypeConverter TypeConverter { get; }

    /// Provides a mechanism for modifying or enhancing the logging behavior within the context of test execution.
    /// This property allows dynamic customization of log content, format, or behavior during runtime execution of tests.
    /// By integrating a custom implementation of `ILogModifier`, test suites can gain fine-grained control
    /// over how log data is processed, enabling use cases such as filtering or restructuring log output.
    public ILogModifier LogModifier { get; }

    /// Provides a mechanism for building and managing namespace context configurations.
    /// This property enables the definition and customization of XML namespaces and their prefixes
    /// within the application or testing environment. It ensures consistent resolution of namespaces
    /// across different XML-related operations performed by the system.
    public NamespaceContextBuilder NamespaceContextBuilder { get; }

    /// Holds the set of configuration classes registered with the context.
    /// This property is used to track and prevent duplicate registrations of
    /// configuration classes during the initialization or parsing of configurations.
    /// It ensures that each configuration class is processed only once, maintaining
    /// consistency and avoiding redundancy within the system.
    public HashSet<Type> ConfigurationClasses { get; } = [];

    /// <summary>
    ///     Adds a message listener to the context, enabling it to receive notifications
    ///     about inbound and outbound message events.
    /// </summary>
    /// <param name="listener">The message listener to be added.</param>
    public void AddMessageListener(IAsyncMessageListener listener)
    {
        MessageListeners.AddMessageListener(listener);
    }

    /// <summary>
    ///     Adds a test action listener to the context, enabling it to receive notifications
    ///     about the start, finish, and skip events of test actions.
    /// </summary>
    /// <param name="listener">The test action listener to be registered.</param>
    public void AddTestActionListener(IAsyncTestActionListener listener)
    {
        TestActionListeners.AddTestActionListener(listener);
    }

    /// <summary>
    ///     Adds a test listener to the current context, allowing it to receive test events.
    /// </summary>
    /// <param name="testListener">The test listener to be added.</param>
    public void AddTestListener(IAsyncTestListener testListener)
    {
        TestListeners.AddTestListener(testListener);
    }

    /// <summary>
    ///     Adds a test reporter to the current context.
    /// </summary>
    /// <param name="testReporter">The test reporter to be added.</param>
    public void AddTestReporter(IAsyncTestReporter testReporter)
    {
        TestReporters.AddTestReporter(testReporter);
    }

    /// <summary>
    ///     Adds a test suite listener to the internal collection of test suite listeners for monitoring test suite events.
    /// </summary>
    /// <param name="suiteListener">The test suite listener to be added.</param>
    public void AddTestSuiteListener(IAsyncTestSuiteListener suiteListener)
    {
        SuiteListeners.AddTestSuiteListener(suiteListener);
    }

    /// <summary>
    ///     Binds a specified name to an associated value within the context,
    ///     enabling retrieval or association of references for further operations.
    /// </summary>
    /// <param name="name">The unique name to be used for binding the value.</param>
    /// <param name="value">The object value to be associated with the provided name.</param>
    public void Bind(string name, object value)
    {
        if (ReferenceResolver == null)
        {
            return;
        }

        ReferenceResolver.Bind(name, value);

        // Check if value is a MessageValidator
        if (value is IMessageValidator<IValidationContext> validator)
        {
            MessageValidatorRegistry.AddMessageValidator(name, validator);
        }
    }


    /// Creates a new test context.
    /// @return the new agenix test context.
    /// /
    public TestContext CreateTestContext()
    {
        return ContextFactory.GetObject();
    }


    /// <summary>
    ///     Initializing method loads default configuration class and reads component definitions such as test listeners and
    ///     test context factory.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="TypeLoadException"></exception>
    /// <exception cref="AgenixSystemException"></exception>
    public static AgenixContext Create()
    {
        var context = Builder.DefaultContext().Build();

        if (string.IsNullOrEmpty(AgenixSettings.DefaultConfigClass()))
        {
            return context;
        }

        try
        {
            var configType = Type.GetType(AgenixSettings.DefaultConfigClass());
            if (configType == null)
            {
                throw new TypeLoadException("Type not found.");
            }

            context.ParseConfiguration(configType);
        }
        catch (Exception ex) when (ex is TypeLoadException or NullReferenceException)
        {
            throw new AgenixSystemException("Failed to instantiate custom configuration class", ex);
        }

        return context;
    }

    /// <summary>
    ///     Parse given configuration class and bind annotated fields, methods to reference registry.
    /// </summary>
    /// <param name="configClass"></param>
    public void ParseConfiguration(Type configClass)
    {
        if (!ConfigurationClasses.Add(configClass))
        {
            return;
        }

        AgenixAnnotations.ParseConfiguration(configClass, this);
    }

    /// <summary>
    ///     Parse given configuration class and bind annotated fields, methods to reference registry.
    /// </summary>
    /// <param name="configuration"></param>
    public void ParseConfiguration(object configuration)
    {
        if (!ConfigurationClasses.Add(configuration.GetType()))
        {
            return;
        }

        AgenixAnnotations.ParseConfiguration(configuration, this);
    }

    /// <summary>
    ///     Retrieves the current collection of test results from the test reporters
    ///     associated with the context.
    /// </summary>
    /// <returns>The current set of test results.</returns>
    public TestResults GetTestResults()
    {
        return TestReporters.GetTestResults();
    }

    /// <summary>
    ///     Compares given test results with the current test results and adds any new results
    ///     that are not present in the existing collection.
    /// </summary>
    /// <param name="testResults">The test results to be handled and compared with the current results.</param>
    public void HandleTestResults(TestResults testResults)
    {
        if (!GetTestResults().Equals(testResults))
        {
            testResults.DoWithResults(result => GetTestResults().AddResult(result));
        }
    }

    /// <summary>
    ///     Adds a component to the context, binding it to a specific name and registering it with the appropriate registries
    ///     and listeners.
    /// </summary>
    /// <param name="name">The name to bind the component with.</param>
    /// <param name="component">The component to be added and processed for registration and initialization.</param>
    public void AddComponent(string name, object component)
    {
        if (component is InitializingPhase c)
        {
            c.Initialize();
        }

        ReferenceResolver.Bind(name, component);

        if (component is IMessageValidator<IValidationContext> messageValidator)
        {
            MessageValidatorRegistry.AddMessageValidator(name, messageValidator);
            ContextFactory.MessageValidatorRegistry.AddMessageValidator(name, messageValidator);
        }

        if (component is IMessageProcessor messageProcessor)
        {
            MessageProcessors.AddMessageProcessor(messageProcessor);
            ContextFactory.MessageProcessors.AddMessageProcessor(messageProcessor);
        }

        if (component is IAsyncTestSuiteListener suiteListener)
        {
            SuiteListeners.AddTestSuiteListener(suiteListener);
        }

        if (component is IAsyncTestListener testListener)
        {
            TestListeners.AddTestListener(testListener);
            ContextFactory.TestListeners.AddTestListener(testListener);
        }

        if (component is IAsyncTestReporter testReporter)
        {
            TestReporters.AddTestReporter(testReporter);
        }

        if (component is IAsyncTestActionListener testActionListener)
        {
            TestActionListeners.AddTestActionListener(testActionListener);
            ContextFactory.TestActionListeners.AddTestActionListener(testActionListener);
        }

        if (component is IAsyncMessageListener messageListener)
        {
            MessageListeners.AddMessageListener(messageListener);
            ContextFactory.MessageListeners.AddMessageListener(messageListener);
        }

        if (component is IBeforeTest beforeTest)
        {
            ContextFactory.BeforeTest.Add(beforeTest);
        }

        if (component is IAfterTest afterTest)
        {
            ContextFactory.AfterTest.Add(afterTest);
        }

        if (component is IBeforeSuite beforeSuiteComponent)
        {
            BeforeSuites.Add(beforeSuiteComponent);
        }

        if (component is IAfterSuite afterSuiteComponent)
        {
            AfterSuites.Add(afterSuiteComponent);
        }

        if (component is FunctionLibrary library)
        {
            FunctionRegistry.AddFunctionLibrary(library);
            ContextFactory.FunctionRegistry.AddFunctionLibrary(library);
        }

        if (component is ValidationMatcherLibrary validationLibrary)
        {
            ValidationMatcherRegistry.AddValidationMatcherLibrary(validationLibrary);
            ContextFactory.ValidationMatcherRegistry.AddValidationMatcherLibrary(validationLibrary);
        }

        if (component is GlobalVariables globalVariables)
        {
            ContextFactory.GlobalVariables = globalVariables;
        }
    }

    /// <summary>
    ///     Closes the context and all its components.
    /// </summary>
    public void Close()
    {
        // Method intentionally left empty.
    }

    /// <summary>
    ///     Configures and builds an instance of the AgenixContext class by providing a fluent API
    ///     for setting various components, such as listeners, processors, validators, and other dependencies.
    /// </summary>
    public class Builder
    {
        internal readonly List<IAfterSuite> _afterSuite = [];
        internal readonly List<IBeforeSuite> _beforeSuite = [];
        internal readonly HashSet<Type> _configurationClasses = [];
        internal IEndpointFactory _endpointFactory = new DefaultEndpointFactory();

        internal FunctionRegistry _functionRegistry = new DefaultFunctionRegistry();
        internal GlobalVariables _globalVariables = new();
        public ILogModifier _logModifier = new DefaultLogModifier();
        internal AsyncMessageListeners _messageListeners = new();
        internal MessageProcessors _messageProcessors = new();
        internal MessageValidatorRegistry _messageValidatorRegistry = new DefaultMessageValidatorRegistry();
        internal NamespaceContextBuilder _namespaceContextBuilder = new();
        internal IReferenceResolver _referenceResolver = new SimpleReferenceResolver();
        internal AsyncTestActionListeners _testActionListeners = new();
        internal TestContextFactory _testContextFactory;
        internal AsyncTestListeners _testListeners = new();
        internal AsyncTestReporters _testReporters = new DefaultAsyncTestReporters();
        internal AsyncTestSuiteListeners _testSuiteListeners = new();
        internal ITypeConverter _typeConverter = ITypeConverter.LookupDefault();
        internal ValidationMatcherRegistry _validationMatcherRegistry = new DefaultValidationMatcherRegistry();

        /// Builds a default context for the Agenix test infrastructure. The context sets up various listeners,
        /// including test suite listeners, test listeners, test action listeners, and message listeners,
        /// by retrieving them from the default test reporters. This method ensures that the appropriate
        /// listeners are associated with the builder instance, leveraging the default test reporters configured
        /// for the Agenix context.
        /// <returns>A configured Builder instance with default listeners and test reporters set.</returns>
        public static Builder DefaultContext()
        {
            var builder = new Builder();

            var testReporters = builder._testReporters.GetTestReporters();

            foreach (var reporter in testReporters)
            {
                if (reporter is IAsyncTestSuiteListener listener)
                {
                    builder.TestSuiteListener(listener);
                }
            }

            builder.TestSuiteListener(builder._testReporters);

            foreach (var reporter in testReporters)
            {
                if (reporter is IAsyncTestListener listener)
                {
                    builder.TestListener(listener);
                }
            }

            builder.TestListener(builder._testReporters);

            foreach (var reporter in testReporters)
            {
                if (reporter is IAsyncTestActionListener listener)
                {
                    builder.TestActionListener(listener);
                }
            }

            foreach (var reporter in testReporters)
            {
                if (reporter is IAsyncMessageListener listener)
                {
                    builder.MessageListener(listener);
                }
            }

            return builder;
        }

        /// <summary>
        ///     Defines a factory for creating and managing test contexts within the framework,
        ///     allowing for dependency resolution and extensibility.
        /// </summary>
        /// <param name="testContextFactory">The TestContextFactory instance to be utilized for test context creation.</param>
        /// <returns>The builder instance with the specified TestContextFactory set.</returns>
        public Builder TestContextFactory(TestContextFactory testContextFactory)
        {
            _testContextFactory = testContextFactory;
            return this;
        }

        /// <summary>
        ///     Sets and configures the asynchronous test suite listeners for the context.
        /// </summary>
        /// <param name="testSuiteListeners">
        ///     The asynchronous test suite listeners to be assigned, which monitor and respond to lifecycle events
        ///     related to the test suite execution.
        /// </param>
        /// <returns>
        ///     Returns the updated builder instance for further configuration.
        /// </returns>
        public Builder TestSuiteListeners(AsyncTestSuiteListeners testSuiteListeners)
        {
            _testSuiteListeners = testSuiteListeners;
            return this;
        }

        /// <summary>
        ///     Adds a test suite listener to the builder, allowing the system
        ///     to receive events or notifications related to test suite execution.
        /// </summary>
        /// <param name="testSuiteListener">
        ///     The test suite listener to be added.
        /// </param>
        /// <returns>
        ///     The builder instance with the added test suite listener.
        /// </returns>
        public Builder TestSuiteListener(IAsyncTestSuiteListener testSuiteListener)
        {
            _testSuiteListeners.AddTestSuiteListener(testSuiteListener);
            return this;
        }

        /// <summary>
        ///     Configures the builder with a collection of asynchronous test listeners.
        /// </summary>
        /// <param name="testListeners">The asynchronous test listeners to be set in the builder.</param>
        /// <returns>The builder instance with the specified test listeners configured.</returns>
        public Builder TestListeners(AsyncTestListeners testListeners)
        {
            _testListeners = testListeners;
            return this;
        }

        /// <summary>
        ///     Adds a test listener to the test listeners collection, enabling it to react to test-related events and actions.
        /// </summary>
        /// <param name="testListener">The test listener to be added.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        public Builder TestListener(IAsyncTestListener testListener)
        {
            _testListeners.AddTestListener(testListener);
            return this;
        }

        /// <summary>
        ///     Sets the asynchronous test action listeners for the context.
        /// </summary>
        /// <param name="testActionListeners">The asynchronous test action listeners to be associated with the context.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public Builder TestActionListeners(AsyncTestActionListeners testActionListeners)
        {
            _testActionListeners = testActionListeners;
            return this;
        }

        /// <summary>
        ///     Adds a test action listener to the context, enabling it to handle test action events during test execution.
        /// </summary>
        /// <param name="testActionListener">The test action listener to be added.</param>
        /// <returns>The builder instance for chaining additional configuration methods.</returns>
        public Builder TestActionListener(IAsyncTestActionListener testActionListener)
        {
            _testActionListeners.AddTestActionListener(testActionListener);
            return this;
        }

        /// <summary>
        ///     Assigns the specified asynchronous test reporters to the builder,
        ///     enabling reporting functionality for the test framework.
        /// </summary>
        /// <param name="testReporters">The instance of <see cref="AsyncTestReporters" /> to be set.</param>
        /// <returns>An instance of the builder for method chaining.</returns>
        public Builder TestReporters(AsyncTestReporters testReporters)
        {
            _testReporters = testReporters;
            return this;
        }

        /// <summary>
        ///     Adds an asynchronous test reporter to the builder, enabling the reporting of test outcomes.
        /// </summary>
        /// <param name="testReporter">The asynchronous test reporter to be added.</param>
        /// <returns>An updated instance of the builder with the specified test reporter added.</returns>
        public Builder TestReporter(IAsyncTestReporter testReporter)
        {
            _testReporters.AddTestReporter(testReporter);
            return this;
        }

        /// <summary>
        ///     Adds a collection of "before suite" actions to be executed prior to the test suite execution.
        /// </summary>
        /// <param name="beforeSuite">
        ///     A list of objects implementing the <see cref="IBeforeSuite" /> interface representing the
        ///     actions to add.
        /// </param>
        /// <returns>A builder instance for chaining additional configurations.</returns>
        public Builder BeforeSuite(List<IBeforeSuite> beforeSuite)
        {
            _beforeSuite.AddRange(beforeSuite);
            return this;
        }

        /// <summary>
        ///     Adds a pre-suite action to be executed before running the test suite.
        /// </summary>
        /// <param name="beforeSuite">The pre-suite action to be executed.</param>
        /// <returns>The builder instance for chaining additional configurations.</returns>
        public Builder BeforeSuite(IBeforeSuite beforeSuite)
        {
            _beforeSuite.Add(beforeSuite);
            return this;
        }

        /// <summary>
        ///     Adds a collection of after-suite actions to the context, which will be executed
        ///     after the completion of a test suite.
        /// </summary>
        /// <param name="afterSuite">
        ///     The list of after-suite actions to be executed after the test suite concludes.
        /// </param>
        /// <returns>
        ///     The builder instance with the after-suite actions configured, allowing for further configuration chaining.
        /// </returns>
        public Builder AfterSuite(List<IAfterSuite> afterSuite)
        {
            _afterSuite.AddRange(afterSuite);
            return this;
        }

        /// <summary>
        ///     Adds an after-suite action to the context. After-suite actions are executed
        ///     after the completion of the test suite to perform any necessary cleanup or finalization tasks.
        /// </summary>
        /// <param name="afterSuite">The <see cref="IAfterSuite" /> instance representing the after-suite action to be added.</param>
        /// <returns>Returns the builder instance to allow method chaining for additional configurations.</returns>
        public Builder AfterSuite(IAfterSuite afterSuite)
        {
            _afterSuite.Add(afterSuite);
            return this;
        }

        /// <summary>
        ///     Represents a registry for managing and maintaining a collection of functions
        ///     within the application context.
        /// </summary>
        public Builder FunctionRegistry(FunctionRegistry functionRegistry)
        {
            _functionRegistry = functionRegistry;
            return this;
        }

        /// <summary>
        ///     Sets the validation matcher registry for the builder, allowing customization
        ///     of validation matchers used within the context.
        /// </summary>
        /// <param name="validationMatcherRegistry">The validation matcher registry to be set.</param>
        /// <returns>The current instance of the builder.</returns>
        public Builder ValidationMatcherRegistry(ValidationMatcherRegistry validationMatcherRegistry)
        {
            _validationMatcherRegistry = validationMatcherRegistry;
            return this;
        }

        /// <summary>
        ///     Sets the global variables for the builder context, enabling the configuration
        ///     of globally accessible variables throughout the context.
        /// </summary>
        /// <param name="globalVariables">The global variables to be set.</param>
        /// <returns>The builder instance with the updated global variables.</returns>
        public Builder GlobalVariables(GlobalVariables globalVariables)
        {
            _globalVariables = globalVariables;
            return this;
        }

        /// <summary>
        ///     Represents a registry for managing message validators.
        /// </summary>
        public Builder MessageValidatorRegistry(MessageValidatorRegistry messageValidatorRegistry)
        {
            _messageValidatorRegistry = messageValidatorRegistry;
            return this;
        }

        /// <summary>
        ///     Sets the collection of asynchronous message listeners for the context's message processing operations.
        /// </summary>
        /// <param name="messageListeners">
        ///     The <see cref="AsyncMessageListeners" /> instance that contains the asynchronous message
        ///     listeners to be used.
        /// </param>
        /// <returns>The current instance of the <see cref="Builder" /> for method chaining.</returns>
        public Builder MessageListeners(AsyncMessageListeners messageListeners)
        {
            _messageListeners = messageListeners;
            return this;
        }

        /// <summary>
        ///     Represents a listener that can be implemented to handle asynchronous message events
        ///     within the Agenix framework.
        /// </summary>
        public Builder MessageListener(IAsyncMessageListener messageListeners)
        {
            _messageListeners.AddMessageListener(messageListeners);
            return this;
        }

        /// <summary>
        ///     Sets the endpoint factory to be used for creating endpoint instances in the context.
        /// </summary>
        /// <param name="endpointFactory">The endpoint factory implementation to be used.</param>
        /// <returns>The current instance of the builder for method chaining.</returns>
        public Builder EndpointFactory(IEndpointFactory endpointFactory)
        {
            _endpointFactory = endpointFactory;
            return this;
        }

        /// <summary>
        ///     Sets the reference resolver for the builder, enabling it to resolve references as needed.
        /// </summary>
        /// <param name="referenceResolver">An instance of <see cref="IReferenceResolver" /> used for resolving references.</param>
        /// <returns>The current instance of the builder for method chaining.</returns>
        public Builder ReferenceResolver(IReferenceResolver referenceResolver)
        {
            _referenceResolver = referenceResolver;
            return this;
        }

        /// <summary>
        ///     Sets the message processors for the context, enabling custom processing
        ///     of messages within the framework.
        /// </summary>
        /// <param name="messageProcessors">
        ///     The instance of <see cref="MessageProcessors" /> to be used for handling message
        ///     processing.
        /// </param>
        /// <returns>The builder instance with the updated message processors configuration.</returns>
        public Builder MessageProcessors(MessageProcessors messageProcessors)
        {
            _messageProcessors = messageProcessors;
            return this;
        }

        /// <summary>
        ///     Sets the type converter to be used for value conversion operations in the context builder.
        /// </summary>
        /// <param name="converter">The type converter implementation to be set.</param>
        /// <returns>The current instance of the builder for chaining further configurations.</returns>
        public Builder TypeConverter(ITypeConverter converter)
        {
            _typeConverter = converter;
            return this;
        }

        /// <summary>
        ///     Sets the log modifier for the context, allowing customization of log behavior.
        /// </summary>
        /// <param name="modifier">The log modifier to be applied to the context.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public Builder LogModifier(ILogModifier modifier)
        {
            _logModifier = modifier;
            return this;
        }

        /// <summary>
        ///     Loads a configuration class into the builder, enabling it to initialize
        ///     and incorporate the specified configuration during the context setup process.
        /// </summary>
        /// <param name="configClass">The type of the configuration class to load.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public Builder LoadConfiguration(Type configClass)
        {
            _configurationClasses.Add(configClass);
            return this;
        }

        /// <summary>
        ///     Configures the builder with the specified namespace context builder, which facilitates managing XML namespace
        ///     contexts.
        /// </summary>
        /// <param name="namespaceContextBuilder">The namespace context builder to be set.</param>
        /// <returns>An updated instance of the builder with the specified namespace context builder.</returns>
        public Builder NamespaceContextBuilder(NamespaceContextBuilder namespaceContextBuilder)
        {
            _namespaceContextBuilder = namespaceContextBuilder;
            return this;
        }

        /// <summary>
        ///     Builds and returns an instance of the AgenixContext by aggregating the configured
        ///     components such as function registry, global variables, test listeners,
        ///     message processors, and other necessary dependencies.
        /// </summary>
        /// <returns>The constructed instance of the AgenixContext.</returns>
        public AgenixContext Build()
        {
            if (_testContextFactory != null)
            {
                return new AgenixContext(this);
            }

            _testContextFactory = Core.TestContextFactory.NewInstance();

            _testContextFactory.FunctionRegistry = _functionRegistry;
            _testContextFactory.ValidationMatcherRegistry = _validationMatcherRegistry;
            _testContextFactory.GlobalVariables = _globalVariables;
            _testContextFactory.MessageValidatorRegistry = _messageValidatorRegistry;
            _testContextFactory.TestListeners = _testListeners;
            _testContextFactory.TestActionListeners = _testActionListeners;
            _testContextFactory.MessageListeners = _messageListeners;
            _testContextFactory.MessageProcessors = _messageProcessors;
            _testContextFactory.EndpointFactory = _endpointFactory;
            _testContextFactory.SetReferenceResolver(_referenceResolver);
            _testContextFactory.TypeConverter = _typeConverter;
            _testContextFactory.LogModifier = _logModifier;
            _testContextFactory.NamespaceContextBuilder = _namespaceContextBuilder;

            return new AgenixContext(this);
        }
    }
}
