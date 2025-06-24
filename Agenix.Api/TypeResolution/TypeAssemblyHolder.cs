#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
