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

using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Functions;

/// <summary>
///     General function interface.
/// </summary>
public interface IFunction
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IFunction));

    /// <summary>
    ///     Collection of function implementations.
    /// </summary>
    private static readonly IDictionary<string, IFunction> Functions = new Dictionary<string, IFunction>();

    /// <summary>
    ///     Represents the resource path for function extensions.
    /// </summary>
    static string ResourcePath => "Extension/agenix/function";

    /// <summary>
    ///     Retrieves a collection of registered function implementations, resolving them if not already loaded.
    /// </summary>
    /// <returns>A dictionary where the keys are function names and the values are the corresponding function implementations.</returns>
    static IDictionary<string, IFunction> Lookup()
    {
        if (Functions.Count != 0)
        {
            return Functions;
        }

        var resolvedFunctions = new ResourcePathTypeResolver().ResolveAll<dynamic>(ResourcePath);

        foreach (var kvp in resolvedFunctions)
        {
            Functions[kvp.Key] = kvp.Value;
        }

        if (!Log.IsEnabled(LogLevel.Debug))
        {
            return Functions;
        }

        {
            foreach (var kvp in Functions)
            {
                Log.LogDebug("Found function '{KvpKey}' as {Type}", kvp.Key, kvp.Value.GetType());
            }
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
