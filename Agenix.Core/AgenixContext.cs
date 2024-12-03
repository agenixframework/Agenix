using System;
using System.Collections.Generic;
using Agenix.Core.Annotations;
using Agenix.Core.Common;
using Agenix.Core.Container;
using Agenix.Core.Endpoint;
using Agenix.Core.Exceptions;
using Agenix.Core.Functions;
using Agenix.Core.Log;
using Agenix.Core.Message;
using Agenix.Core.Report;
using Agenix.Core.Spi;
using Agenix.Core.Util;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Context;
using Agenix.Core.Validation.Matcher;

namespace Agenix.Core;

/// <summary>
///     Default Agenix context implementation holds basic components used in Agenix.
/// </summary>
public class AgenixContext : ITestListenerAware, ITestActionListenerAware, ITestSuiteListenerAware, ITestReporterAware,
    IMessageListenerAware, IReferenceRegistry
{
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

        ContextFactory = builder._testContextFactory;

        foreach (var config in builder._configurationClasses) ParseConfiguration(config);
    }

    public TestContextFactory ContextFactory { get; }

    public TestSuiteListeners SuiteListeners { get; }

    public TestListeners TestListeners { get; }

    public TestActionListeners TestActionListeners { get; }

    public TestReporters TestReporters { get; }

    public List<IBeforeSuite> BeforeSuites { get; }

    /// A list of `IAfterSuite` instances executed after a test suite completes.
    /// This property holds a collection of actions that are triggered after the entire
    /// test suite has finished execution, allowing for finalization or cleanup operations.
    /// Each `IAfterSuite` instance determines whether it should execute based on the
    /// context of the test suite that just ran, such as the suite's name or the groups included.
    public List<IAfterSuite> AfterSuites { get; }

    public FunctionRegistry FunctionRegistry { get; }

    public ValidationMatcherRegistry ValidationMatcherRegistry { get; }

    public GlobalVariables GlobalVariables { get; }

    public MessageValidatorRegistry MessageValidatorRegistry { get; }

    public MessageListeners MessageListeners { get; }

    public IEndpointFactory EndpointFactory { get; }

    public IReferenceResolver ReferenceResolver { get; }

    public MessageProcessors MessageProcessors { get; }

    public ITypeConverter TypeConverter { get; }

    public ILogModifier LogModifier { get; }

    public HashSet<Type> ConfigurationClasses { get; } = [];

    /// <summary>
    ///     Adds a message listener to the context, enabling it to receive notifications
    ///     about inbound and outbound message events.
    /// </summary>
    /// <param name="listener">The message listener to be added.</param>
    public void AddMessageListener(IMessageListener listener)
    {
        MessageListeners.AddMessageListener(listener);
    }

    public void Bind(string name, object value)
    {
        if (ReferenceResolver != null)
        {
            ReferenceResolver.Bind(name, value);

            // Check if value is a MessageValidator
            if (value is IMessageValidator<IValidationContext> validator)
                MessageValidatorRegistry.AddMessageValidator(name, validator);
        }
    }

    /// <summary>
    ///     Adds a test action listener to the context, enabling it to receive notifications
    ///     about the start, finish, and skip events of test actions.
    /// </summary>
    /// <param name="listener">The test action listener to be registered.</param>
    public void AddTestActionListener(ITestActionListener listener)
    {
        TestActionListeners.AddTestActionListener(listener);
    }

    /// <summary>
    ///     Adds a test listener to the current context, allowing it to receive test events.
    /// </summary>
    /// <param name="testListener">The test listener to be added.</param>
    public void AddTestListener(ITestListener testListener)
    {
        TestListeners.AddTestListener(testListener);
    }

    /// <summary>
    ///     Adds a test reporter to the current context.
    /// </summary>
    /// <param name="testReporter">The test reporter to be added.</param>
    public void AddTestReporter(ITestReporter testReporter)
    {
        TestReporters.AddTestReporter(testReporter);
    }

    /// <summary>
    ///     Adds a test suite listener to the internal collection of test suite listeners for monitoring test suite events.
    /// </summary>
    /// <param name="suiteListener">The test suite listener to be added.</param>
    public void AddTestSuiteListener(ITestSuiteListener suiteListener)
    {
        SuiteListeners.AddTestSuiteListener(suiteListener);
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
    /// <exception cref="CoreSystemException"></exception>
    public static AgenixContext Create()
    {
        var context = Builder.DefaultContext().Build();

        if (string.IsNullOrEmpty(CoreSettings.DefaultConfigClass)) return context;

        try
        {
            var configType = Type.GetType(CoreSettings.DefaultConfigClass);
            if (configType == null) throw new TypeLoadException("Type not found.");
            context.ParseConfiguration(configType);
        }
        catch (Exception ex) when (ex is TypeLoadException or NullReferenceException)
        {
            throw new CoreSystemException("Failed to instantiate custom configuration class", ex);
        }

        return context;
    }

    /// <summary>
    ///     Parse given configuration class and bind annotated fields, methods to reference registry.
    /// </summary>
    /// <param name="configClass"></param>
    public void ParseConfiguration(Type configClass)
    {
        if (ConfigurationClasses.Contains(configClass)) return;

        ConfigurationClasses.Add(configClass);
        AgenixAnnotations.ParseConfiguration(configClass, this);
    }

    /// <summary>
    ///     Parse given configuration class and bind annotated fields, methods to reference registry.
    /// </summary>
    /// <param name="configuration"></param>
    public void ParseConfiguration(object configuration)
    {
        if (ConfigurationClasses.Contains(configuration.GetType())) return;

        ConfigurationClasses.Add(configuration.GetType());
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
            testResults.DoWithResults(result => GetTestResults().AddResult(result));
    }

    /// <summary>
    ///     Adds a component to the context, binding it to a specific name and registering it with the appropriate registries
    ///     and listeners.
    /// </summary>
    /// <param name="name">The name to bind the component with.</param>
    /// <param name="component">The component to be added and processed for registration and initialization.</param>
    public void AddComponent(string name, object component)
    {
        if (component is InitializingPhase c) c.Initialize();

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

        if (component is ITestSuiteListener suiteListener) SuiteListeners.AddTestSuiteListener(suiteListener);

        if (component is ITestListener testListener)
        {
            TestListeners.AddTestListener(testListener);
            ContextFactory.TestListeners.AddTestListener(testListener);
        }

        if (component is ITestReporter testReporter) TestReporters.AddTestReporter(testReporter);

        if (component is ITestActionListener testActionListener)
        {
            TestActionListeners.AddTestActionListener(testActionListener);
            ContextFactory.TestActionListeners.AddTestActionListener(testActionListener);
        }

        if (component is IMessageListener messageListener)
        {
            MessageListeners.AddMessageListener(messageListener);
            ContextFactory.MessageListeners.AddMessageListener(messageListener);
        }

        if (component is IBeforeTest beforeTest) ContextFactory.BeforeTest.Add(beforeTest);

        if (component is IAfterTest afterTest) ContextFactory.AfterTest.Add(afterTest);

        if (component is IBeforeSuite beforeSuiteComponent) BeforeSuites.Add(beforeSuiteComponent);

        if (component is IAfterSuite afterSuiteComponent) AfterSuites.Add(afterSuiteComponent);

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

        if (component is GlobalVariables globalVariables) ContextFactory.GlobalVariables = globalVariables;
    }

    /// <summary>
    ///     Closes the context and all its components.
    /// </summary>
    public void Close()
    {
    }

    public class Builder
    {
        internal readonly List<IAfterSuite> _afterSuite = [];
        internal readonly List<IBeforeSuite> _beforeSuite = [];
        internal readonly HashSet<Type> _configurationClasses = [];
        internal IEndpointFactory _endpointFactory = new DefaultEndpointFactory();

        internal FunctionRegistry _functionRegistry = new DefaultFunctionRegistry();
        internal GlobalVariables _globalVariables = new();
        public ILogModifier _logModifier = new DefaultLogModifier();
        internal MessageListeners _messageListeners = new();
        internal MessageProcessors _messageProcessors = new();
        internal MessageValidatorRegistry _messageValidatorRegistry = new DefaultMessageValidatorRegistry();
        internal IReferenceResolver _referenceResolver = new SimpleReferenceResolver();
        internal TestActionListeners _testActionListeners = new();
        internal TestContextFactory _testContextFactory;
        internal TestListeners _testListeners = new();
        internal TestReporters _testReporters = new DefaultTestReporters();
        internal TestSuiteListeners _testSuiteListeners = new();
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
                if (reporter is ITestSuiteListener listener)
                    builder.TestSuiteListener(listener);

            builder.TestSuiteListener(builder._testReporters);

            foreach (var reporter in testReporters)
                if (reporter is ITestListener listener)
                    builder.TestListener(listener);

            builder.TestListener(builder._testReporters);

            foreach (var reporter in testReporters)
                if (reporter is ITestActionListener listener)
                    builder.TestActionListener(listener);

            foreach (var reporter in testReporters)
                if (reporter is IMessageListener listener)
                    builder.MessageListener(listener);

            return builder;
        }

        public Builder TestContextFactory(TestContextFactory testContextFactory)
        {
            _testContextFactory = testContextFactory;
            return this;
        }

        public Builder TestSuiteListeners(TestSuiteListeners testSuiteListeners)
        {
            _testSuiteListeners = testSuiteListeners;
            return this;
        }

        public Builder TestSuiteListener(ITestSuiteListener testSuiteListener)
        {
            _testSuiteListeners.AddTestSuiteListener(testSuiteListener);
            return this;
        }

        public Builder TestListeners(TestListeners testListeners)
        {
            _testListeners = testListeners;
            return this;
        }

        public Builder TestListener(ITestListener testListener)
        {
            _testListeners.AddTestListener(testListener);
            return this;
        }

        public Builder TestActionListeners(TestActionListeners testActionListeners)
        {
            _testActionListeners = testActionListeners;
            return this;
        }

        public Builder TestActionListener(ITestActionListener testActionListener)
        {
            _testActionListeners.AddTestActionListener(testActionListener);
            return this;
        }

        public Builder TestReporters(TestReporters testReporters)
        {
            _testReporters = testReporters;
            return this;
        }

        public Builder TestReporter(ITestReporter testReporter)
        {
            _testReporters.AddTestReporter(testReporter);
            return this;
        }

        public Builder BeforeSuite(List<IBeforeSuite> beforeSuite)
        {
            _beforeSuite.AddRange(beforeSuite);
            return this;
        }

        public Builder BeforeSuite(IBeforeSuite beforeSuite)
        {
            _beforeSuite.Add(beforeSuite);
            return this;
        }

        public Builder AfterSuite(List<IAfterSuite> afterSuite)
        {
            _afterSuite.AddRange(afterSuite);
            return this;
        }

        public Builder AfterSuite(IAfterSuite afterSuite)
        {
            _afterSuite.Add(afterSuite);
            return this;
        }

        public Builder FunctionRegistry(FunctionRegistry functionRegistry)
        {
            _functionRegistry = functionRegistry;
            return this;
        }

        public Builder ValidationMatcherRegistry(ValidationMatcherRegistry validationMatcherRegistry)
        {
            _validationMatcherRegistry = validationMatcherRegistry;
            return this;
        }

        public Builder GlobalVariables(GlobalVariables globalVariables)
        {
            _globalVariables = globalVariables;
            return this;
        }

        public Builder MessageValidatorRegistry(MessageValidatorRegistry messageValidatorRegistry)
        {
            _messageValidatorRegistry = messageValidatorRegistry;
            return this;
        }

        public Builder MessageListeners(MessageListeners messageListeners)
        {
            _messageListeners = messageListeners;
            return this;
        }

        public Builder MessageListener(IMessageListener messageListeners)
        {
            _messageListeners.AddMessageListener(messageListeners);
            return this;
        }

        public Builder EndpointFactory(IEndpointFactory endpointFactory)
        {
            _endpointFactory = endpointFactory;
            return this;
        }

        public Builder ReferenceResolver(IReferenceResolver referenceResolver)
        {
            _referenceResolver = referenceResolver;
            return this;
        }

        public Builder MessageProcessors(MessageProcessors messageProcessors)
        {
            _messageProcessors = messageProcessors;
            return this;
        }

        public Builder TypeConverter(ITypeConverter converter)
        {
            _typeConverter = converter;
            return this;
        }

        public Builder LogModifier(ILogModifier modifier)
        {
            _logModifier = modifier;
            return this;
        }

        public Builder LoadConfiguration(Type configClass)
        {
            _configurationClasses.Add(configClass);
            return this;
        }

        public AgenixContext Build()
        {
            if (_testContextFactory == null)
            {
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
            }

            return new AgenixContext(this);
        }
    }
}