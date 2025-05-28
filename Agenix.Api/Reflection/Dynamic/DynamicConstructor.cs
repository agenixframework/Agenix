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

using System.Reflection;
using Agenix.Api.Util;

namespace Agenix.Api.Reflection.Dynamic;

#region IDynamicConstructor interface

/// <summary>
///     Defines constructors that a dynamic constructor class has to implement.
/// </summary>
public interface IDynamicConstructor
{
    /// <summary>
    ///     Invokes dynamic constructor.
    /// </summary>
    /// <param name="arguments">
    ///     Constructor arguments.
    /// </param>
    /// <returns>
    ///     A constructor value.
    /// </returns>
    object Invoke(object[] arguments);
}

#endregion

#region Safe wrapper

/// <summary>
///     Safe wrapper for the dynamic constructor.
/// </summary>
/// <remarks>
///     <see cref="SafeConstructor" /> will attempt to use dynamic
///     constructor if possible, but it will fall back to standard
///     reflection if necessary.
/// </remarks>
public class SafeConstructor : IDynamicConstructor
{
    private readonly ConstructorDelegate _constructor;
    private ConstructorInfo _constructorInfo;

    /// <summary>
    ///     Creates a new instance of the safe constructor wrapper.
    /// </summary>
    /// <param name="constructorInfo">Constructor to wrap.</param>
    public SafeConstructor(ConstructorInfo constructorInfo)
    {
        _constructorInfo = constructorInfo;
        _constructor = GetOrCreateDynamicConstructor(constructorInfo);
    }


    /// <summary>
    ///     Invokes dynamic constructor.
    /// </summary>
    /// <param name="arguments">
    ///     Constructor arguments.
    /// </param>
    /// <returns>
    ///     A constructor value.
    /// </returns>
    public object Invoke(object[] arguments)
    {
        return _constructor(arguments);
    }

    #region Generated Function Cache

    private static readonly IDictionary<ConstructorInfo, ConstructorDelegate> constructorCache =
        new Dictionary<ConstructorInfo, ConstructorDelegate>();

    /// <summary>
    ///     Obtains cached constructor info or creates a new entry, if none is found.
    /// </summary>
    private static ConstructorDelegate GetOrCreateDynamicConstructor(ConstructorInfo constructorInfo)
    {
        ConstructorDelegate method;
        if (!constructorCache.TryGetValue(constructorInfo, out method))
        {
            method = DynamicReflectionManager.CreateConstructor(constructorInfo);
            lock (constructorCache)
            {
                constructorCache[constructorInfo] = method;
            }
        }

        return method;
    }

    #endregion
}

#endregion

/// <summary>
///     Factory class for dynamic constructors.
/// </summary>
public class DynamicConstructor : BaseDynamicMember
{
    /// <summary>
    ///     Creates dynamic constructor instance for the specified <see cref="ConstructorInfo" />.
    /// </summary>
    /// <param name="constructorInfo">Constructor info to create dynamic constructor for.</param>
    /// <returns>Dynamic constructor for the specified <see cref="ConstructorInfo" />.</returns>
    public static IDynamicConstructor Create(ConstructorInfo constructorInfo)
    {
        AssertUtils.ArgumentNotNull(constructorInfo, "You cannot create a dynamic constructor for a null value.");

        return new SafeConstructor(constructorInfo);
    }
}
