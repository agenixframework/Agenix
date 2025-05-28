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

namespace Agenix.Api.Config;

/// <summary>
///     Defines contract that different variable sources have to implement.
/// </summary>
/// <remarks>
///     <p>
///         The "variable sources" are objects containing name-value pairs
///         that allow a variable value to be retrieved for the given name.
///     </p>
///     <p>
///         Out of the box, Spring.NET supports a number of variable sources,
///         that allow users to obtain variable values from .NET config files,
///         Java-style property files, environment, registry, etc.
///     </p>
///     <p>
///         Users can always write their own variable sources implementations,
///         that will allow them to load variable values from the database or
///         other proprietary data source.
///     </p>
/// </remarks>
/// <seealso cref="ConfigSectionVariableSource" />
/// <seealso cref="PropertyFileVariableSource" />
/// <seealso cref="EnvironmentVariableSource" />
/// <seealso cref="CommandLineArgsVariableSource" />
/// <seealso cref="RegistryVariableSource" />
/// <seealso cref="SpecialFolderVariableSource" />
public interface IVariableSource
{
    /// <summary>
    ///     Before requesting a variable resolution, a client should
    ///     ask, whether the source can resolve a particular variable name.
    /// </summary>
    /// <param name="name">the name of the variable to resolve</param>
    /// <returns><c>true</c> if the variable can be resolved, <c>false</c> otherwise</returns>
    bool CanResolveVariable(string name);

    /// <summary>
    ///     Resolves variable value for the specified variable name.
    /// </summary>
    /// <param name="name">
    ///     The name of the variable to resolve.
    /// </param>
    /// <returns>
    ///     The variable value if able to resolve, <c>null</c> otherwise.
    /// </returns>
    string ResolveVariable(string name);
}
