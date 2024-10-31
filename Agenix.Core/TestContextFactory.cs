using Agenix.Core.Functions;
using Agenix.Core.Log;
using Agenix.Core.Validation.Matcher;

namespace Agenix.Core;

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

    public GlobalVariables GlobalVariables { get; set; } = new();

    /// <summary>
    ///     Gets a new instance of TestContext with default/ core function library initialized.
    /// </summary>
    /// <returns>new instance of TestContext</returns>
    public TestContext GetObject()
    {
        var context = new TestContext
        {
            FunctionRegistry = FunctionRegistry,
            ValidationMatcherRegistry = ValidationMatcherRegistry
        };

        context.SetGlobalVariables(GlobalVariables);

        if (LogModifier != null) context.LogModifier = LogModifier;

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
            GlobalVariables = new GlobalVariables()
        };

        return factory;
    }

    public static TestContext CopyOf(TestContext context)
    {
        TestContext result = new()
        {
            FunctionRegistry = context.FunctionRegistry,
            ValidationMatcherRegistry = context.ValidationMatcherRegistry,
            LogModifier = context.LogModifier,
            MessageStore = context.MessageStore
        };

        foreach (var kvp in context.GetVariables()) result.GetVariables()[kvp.Key] = kvp.Value;

        result.SetGlobalVariables(new GlobalVariables.Builder()
            .WithVariables(context.GetGlobalVariables())
            .Build());

        return result;
    }
}