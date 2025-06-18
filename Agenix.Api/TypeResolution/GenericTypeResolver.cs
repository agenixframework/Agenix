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
///     Resolves a generic <see cref="System.Type" /> by name.
/// </summary>
public class GenericTypeResolver : TypeResolver
{
    /// <summary>
    ///     Resolves the supplied generic <paramref name="typeName" /> to a
    ///     <see cref="System.Type" /> instance.
    /// </summary>
    /// <param name="typeName">
    ///     The unresolved (possibly generic) name of a <see cref="System.Type" />.
    /// </param>
    /// <returns>
    ///     A resolved <see cref="System.Type" /> instance.
    /// </returns>
    /// <exception cref="System.TypeLoadException">
    ///     If the supplied <paramref name="typeName" /> could not be resolved
    ///     to a <see cref="System.Type" />.
    /// </exception>
    public override Type Resolve(string typeName)
    {
        if (StringUtils.IsNullOrEmpty(typeName))
        {
            throw BuildTypeLoadException(typeName);
        }

        var genericInfo = new GenericArgumentsHolder(typeName);
        Type type = null;
        try
        {
            if (genericInfo.ContainsGenericArguments)
            {
                type = TypeResolutionUtils.ResolveType(genericInfo.GenericTypeName);
                if (!genericInfo.IsGenericDefinition)
                {
                    var unresolvedGenericArgs = genericInfo.GetGenericArguments();
                    var genericArgs = new Type[unresolvedGenericArgs.Length];
                    for (var i = 0; i < unresolvedGenericArgs.Length; i++)
                    {
                        genericArgs[i] = TypeResolutionUtils.ResolveType(unresolvedGenericArgs[i]);
                    }

                    type = type.MakeGenericType(genericArgs);
                }

                if (genericInfo.IsArrayDeclaration)
                {
                    typeName = string.Format("{0}{1},{2}", type.FullName, genericInfo.GetArrayDeclaration(),
                        type.Assembly.FullName);
                    type = null;
                }
            }
        }
        catch (Exception ex)
        {
            if (ex is TypeLoadException)
            {
                throw;
            }

            throw BuildTypeLoadException(typeName, ex);
        }

        if (type == null)
        {
            type = base.Resolve(typeName);
        }

        return type;
    }
}
