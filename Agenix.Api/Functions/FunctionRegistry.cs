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
///     Function registry holding all available function libraries.
/// </summary>
public class FunctionRegistry
{
    /// <summary>
    ///     The list of libraries providing custom functions
    /// </summary>
    private List<FunctionLibrary> _functionLibraries = [];

    /// <summary>
    ///     The list of libraries providing custom functions
    /// </summary>
    public List<FunctionLibrary> FunctionLibraries
    {
        get => _functionLibraries;
        set => _functionLibraries = value;
    }

    /// <summary>
    ///     Check if variable expression is a custom function. Expression has to start with one of the registered function
    ///     library prefix.
    /// </summary>
    /// <param name="variableExpression">to be checked</param>
    /// <returns>flag (true/false)</returns>
    public bool IsFunction(string variableExpression)
    {
        return !string.IsNullOrEmpty(variableExpression) &&
               _functionLibraries.Any(c => variableExpression.StartsWith(c.Prefix));
    }

    /// <summary>
    ///     Get library for function prefix.
    /// </summary>
    /// <param name="functionPrefix"> to be searched for</param>
    /// <returns>The FunctionLibrary instance</returns>
    public FunctionLibrary GetLibraryForPrefix(string functionPrefix)
    {
        var functionLibrary = _functionLibraries.FirstOrDefault(f => f.Prefix.Equals(functionPrefix));

        return functionLibrary ??
               throw new NoSuchFunctionLibraryException(
                   "Can not find function library for prefix " + functionPrefix);
    }

    /// <summary>
    ///     Adds the given function library to this registry.
    /// </summary>
    /// <param name="functionLibrary">The function library to add.</param>
    public void AddFunctionLibrary(FunctionLibrary functionLibrary)
    {
        var prefixAlreadyUsed = _functionLibraries.Any(lib => lib.Prefix == functionLibrary.Prefix);

        if (prefixAlreadyUsed)
        {
            throw new AgenixSystemException(
                $"Function library prefix '{functionLibrary.Prefix}' is already bound to another instance. Please choose another prefix.");
        }

        _functionLibraries.Add(functionLibrary);
    }
}
