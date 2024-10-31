using System.Collections.Generic;
using Agenix.Core.Exceptions;

namespace Agenix.Core.Functions;

/// <summary>
///     Library holding a set of functions. Each library defines a function prefix as namespace, so there will be no naming
///     conflicts when using multiple libraries at a time.
/// </summary>
public class FunctionLibrary
{
    /// <summary>
    ///     The Default function prefix
    /// </summary>
    private const string DefaultPrefix = "core:";

    /// <summary>
    ///     The dictionary (map) of functions in this library
    /// </summary>
    private IDictionary<string, IFunction> _members = new Dictionary<string, IFunction>();

    /// <summary>
    ///     Name of function library
    /// </summary>
    private string _name = DefaultPrefix;

    /// <summary>
    ///     Function library prefix
    /// </summary>
    private string _prefix = DefaultPrefix;

    /// <summary>
    ///     The dictionary (map) of functions in this library
    /// </summary>
    public IDictionary<string, IFunction> Members
    {
        get => _members;
        set => _members = value;
    }

    /// <summary>
    ///     Name of function library
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     Function library prefix
    /// </summary>
    public string Prefix
    {
        get => _prefix;
        set => _prefix = value;
    }

    /// <summary>
    ///     Try to find function in library by name.
    /// </summary>
    /// <param name="functionName">The  function name.</param>
    /// <returns>The function instance.</returns>
    public IFunction GetFunction(string functionName)
    {
        if (!_members.ContainsKey(functionName))
            throw new NoSuchFunctionException("Can not find function '" + functionName + "' in library " + _name +
                                              " (" + _prefix + ")");

        return _members[functionName];
    }

    /// <summary>
    ///     Does this function library know a function with the given name.
    /// </summary>
    /// <param name="functionName">The name to search for.</param>
    /// <returns>The flag to mark existence.</returns>
    public bool KnowsFunction(string functionName)
    {
        var functionPrefix = functionName.Substring(0, functionName.IndexOf(':') + 1);

        if (!functionPrefix.Equals(_prefix)) return false;

        return _members.ContainsKey(
            functionName.Substring(functionName.IndexOf(':') + 1, functionName.IndexOf('(')));
    }
}