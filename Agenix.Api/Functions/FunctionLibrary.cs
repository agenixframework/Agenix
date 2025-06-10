#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using Agenix.Api.Exceptions;

namespace Agenix.Api.Functions;

/// <summary>
///     Library holding a set of functions. Each library defines a function prefix as namespace, so there will be no naming
///     conflicts when using multiple libraries at a time.
/// </summary>
public class FunctionLibrary
{
    /// <summary>
    ///     The Default function prefix
    /// </summary>
    private const string DefaultPrefix = "agenix:";

    /// <summary>
    ///     The dictionary (map) of functions in this library
    /// </summary>
    private IDictionary<string, IFunction> _members =
        new Dictionary<string, IFunction>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Name of a function library
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
        {
            throw new NoSuchFunctionException("Can not find function '" + functionName + "' in library " + _name +
                                              " (" + _prefix + ")");
        }

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

        if (!functionPrefix.Equals(_prefix))
        {
            return false;
        }

        return _members.ContainsKey(
            functionName.Substring(functionName.IndexOf(':') + 1, functionName.IndexOf('(')));
    }
}
