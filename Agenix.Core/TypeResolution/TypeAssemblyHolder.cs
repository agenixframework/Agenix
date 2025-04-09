#region Imports

using System;
using Agenix.Core.Util;

#endregion

namespace Agenix.Core.TypeResolution;

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

    #region Fields

    #endregion

    #region Properties

    /// <summary>
    ///     The (unresolved) type name portion of the original type name.
    /// </summary>
    public string TypeName { get; private set; }

    /// <summary>
    ///     The (unresolved, possibly partial) name of the attandant assembly.
    /// </summary>
    public string AssemblyName { get; private set; }

    /// <summary>
    ///     Is the type name being resolved assembly qualified?
    /// </summary>
    public bool IsAssemblyQualified => StringUtils.HasText(AssemblyName);

    #endregion
}