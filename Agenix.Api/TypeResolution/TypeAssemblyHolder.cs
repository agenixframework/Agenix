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

#region Imports

using Agenix.Api.Util;

#endregion

namespace Agenix.Api.TypeResolution;

/// <summary>
///     Holds data about a <see cref="System.Type" /> and it's
///     attendant <see cref="System.Reflection.Assembly" />.
/// </summary>
public class TypeAssemblyHolder
{
    #region Constants

    /// <summary>
    ///     The string that separates a <see cref="System.Type" /> name
    ///     from the name of it's attendant <see cref="System.Reflection.Assembly" />
    ///     in an assembly qualified type name.
    /// </summary>
    public const string TypeAssemblySeparator = ",";

    #endregion

    #region Constructor (s) / Destructor

    /// <summary>
    ///     Creates a new instance of the TypeAssemblyHolder class.
    /// </summary>
    /// <param name="unresolvedTypeName">
    ///     The unresolved name of a <see cref="System.Type" />.
    /// </param>
    public TypeAssemblyHolder(string unresolvedTypeName)
    {
        SplitTypeAndAssemblyNames(unresolvedTypeName);
    }

    #endregion

    #region Methods

    private void SplitTypeAndAssemblyNames(string originalTypeName)
    {
        // generic types may look like:
        // Spring.Objects.TestGenericObject`2[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]][] , Spring.Core.Tests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
        //
        // start searching for assembly separator after the last bracket if any
        var typeAssemblyIndex = originalTypeName.LastIndexOf(']');
        typeAssemblyIndex =
            originalTypeName.IndexOf(TypeAssemblySeparator, typeAssemblyIndex + 1, StringComparison.Ordinal);
        if (typeAssemblyIndex < 0)
        {
            TypeName = originalTypeName;
        }
        else
        {
            TypeName = originalTypeName.Substring(
                0, typeAssemblyIndex).Trim();
            AssemblyName = originalTypeName.Substring(
                typeAssemblyIndex + 1).Trim();
        }
    }

    #endregion

    #region Properties

    /// <summary>
    ///     The (unresolved) type name portion of the original type name.
    /// </summary>
    public string TypeName { get; private set; }

    /// <summary>
    ///     The (unresolved, possibly partial) name of the attendant assembly.
    /// </summary>
    public string AssemblyName { get; private set; }

    /// <summary>
    ///     Is the type name being resolved assembly qualified?
    /// </summary>
    public bool IsAssemblyQualified => StringUtils.HasText(AssemblyName);

    #endregion
}
