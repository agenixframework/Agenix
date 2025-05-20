using Agenix.Api.Context;
using Agenix.Core.Spi;
using log4net;

namespace Agenix.Api.Functions;

/// <summary>
///     General function interface.
/// </summary>
public interface IFunction
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(IFunction));
    
    /// <summary>
    /// Represents the resource path for function extensions.
    /// </summary>
    static string ResourcePath => "Extension/agenix/function";

    /// <summary>
    /// Collection of function implementations.
    /// </summary>
    private static readonly IDictionary<string, IFunction> Functions = new Dictionary<string, IFunction>();

    /// <summary>
    /// Retrieves a collection of registered function implementations, resolving them if not already loaded.
    /// </summary>
    /// <returns>A dictionary where the keys are function names and the values are the corresponding function implementations.</returns>
    static IDictionary<string, IFunction> Lookup()
    {
        if (Functions.Count != 0) return Functions;

        var resolvedFunctions = new ResourcePathTypeResolver().ResolveAll<dynamic>(ResourcePath);

        foreach (var kvp in resolvedFunctions) Functions[kvp.Key] = kvp.Value;

        if (!Log.IsDebugEnabled) return Functions;
        {
            foreach (var kvp in Functions) Log.Debug($"Found function '{kvp.Key}' as {kvp.Value.GetType()}");
        }
        return Functions;
    }

    /// <summary>
    ///     Method called on execution.
    /// </summary>
    /// <param name="parameterList">The list of function arguments.</param>
    /// <param name="testContext">The test context</param>
    /// <returns>The function result as string.</returns>
    string Execute(List<string> parameterList, TestContext testContext);
}