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

namespace Agenix.Api.TypeResolution;

/// <summary>
///     Resolves a <see cref="System.Type" /> by name.
/// </summary>
/// <remarks>
///     <p>
///         The rationale behind the creation of this interface is to centralise
///         the resolution of type names to <see cref="System.Type" /> instances
///         beyond that offered by the plain vanilla
///         <see cref="System.Type.GetType(string)" /> method call.
///     </p>
/// </remarks>
public interface ITypeResolver
{
    /// <summary>
    ///     Resolves the supplied <paramref name="typeName" /> to a
    ///     <see cref="System.Type" />
    ///     instance.
    /// </summary>
    /// <param name="typeName">
    ///     The (possibly partially assembly qualified) name of a
    ///     <see cref="System.Type" />.
    /// </param>
    /// <returns>
    ///     A resolved <see cref="System.Type" /> instance.
    /// </returns>
    /// <exception cref="System.TypeLoadException">
    ///     If the supplied <paramref name="typeName" /> could not be resolved
    ///     to a <see cref="System.Type" />.
    /// </exception>
    Type Resolve(string typeName);
}
