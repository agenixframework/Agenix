using System.Collections.Generic;
using Agenix.Core.Container;
using Agenix.Core.Functions;
using Agenix.Core.Log;
using Agenix.Core.Report;
using Agenix.Core.Util;
using Agenix.Core.Validation.Matcher;

namespace Agenix.Core;

/// <summary>
///     Factory class to create instances of TestContext with specified configurations.
/// </summary>
public class TestContextFactory
{
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
    private TestListeners TestListeners { get; set; }

    /// <summary>
    ///     Gets or sets the TestActionListeners instance responsible for broadcasting test action events
    ///     to all registered test action listeners.
    /// </summary>
    private TestActionListeners TestActionListeners { get; set; }

    /// <summary>
    ///     Gets or sets the collection of message listeners.
    /// </summary>
    private MessageListeners MessageListeners { get; set; }

    /// <summary>
    ///     A list of actions to be executed prior to the test execution.
    /// </summary>
    public List<IBeforeTest> BeforeTest { get; set; } = [];

    /// <summary>
    ///     Collection of actions to be executed after a test completes.
    /// </summary>
    public List<IAfterTest> AfterTest { get; set; } = [];

    /// <summary>
    ///     Gets a new instance of TestContext with default/ core function library initialized.
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
            BeforeTest = BeforeTest,
            AfterTest = AfterTest
        };

        context.SetGlobalVariables(GlobalVariables);

        if (LogModifier != null) context.LogModifier = LogModifier;

        if (TypeConverter != null) context.TypeConverter = TypeConverter;

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
            MessageListeners = new MessageListeners()
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
            MessageListeners = context.MessageListeners
        };

        foreach (var kvp in context.GetVariables()) result.GetVariables()[kvp.Key] = kvp.Value;

        result.SetGlobalVariables(new GlobalVariables.Builder()
            .WithVariables(context.GetGlobalVariables())
            .Build());

        return result;
    }
}