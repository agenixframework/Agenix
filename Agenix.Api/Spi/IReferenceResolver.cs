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

namespace Agenix.Api.Spi;

/// Provides methods to resolve references by name or type. This interface extends IReferenceRegistry and enables resolution
/// of objects that are bound to a registry. It can resolve single or multiple references based on names and types provided.
public interface IReferenceResolver : IReferenceRegistry
{
    /// Resolves a reference by its name and type.
    /// <param name="names">The name of the reference to resolve.</param>
    /// <returns>The resolved reference object.</returns>
    List<T> Resolve<T>(params string[] names)
    {
        return names.Length > 0 ? Resolve<T>(names, typeof(T)) : [.. ResolveAll<T>().Values];
    }

    /// Resolves a reference by its name and type.
    /// <param name="names">The name of the reference to resolve.</param>
    /// <param name="type">The type of the reference to resolve.</param>
    /// <returns>The resolved reference object.</returns>
    List<T> Resolve<T>(string[] names, Type type)
    {
        List<T> resolved = [];
        resolved.AddRange(names.Select(Resolve<T>));
        return resolved;
    }

    /// Resolves a reference by its name.
    /// <param name="name">The name of the reference to resolve.</param>
    /// <returns>The resolved reference object.</returns>
    object Resolve(string name)
    {
        return Resolve<object>(name);
    }

    /// Resolves a reference by its name and type.
    /// <returns>The resolved reference object.</returns>
    T Resolve<T>();

    /// Resolves a reference by its name and type.
    /// <param name="name">The name of the reference to resolve.</param>
    /// <returns>The resolved reference object.</returns>
    T Resolve<T>(string name);

    /// Resolves all references of a specified type.
    /// <typeparam name="T">The type of the references to resolve.</typeparam>
    /// <returns>A dictionary containing the resolved objects, with their names as keys.</returns>
    Dictionary<string, T> ResolveAll<T>();

    /// Determines if a reference with the specified name can be resolved.
    /// <param name="name">The name of the reference to check.</param>
    /// <returns>True if the reference can be resolved; otherwise, false.</returns>
    bool IsResolvable(string name);

    /// Determines if a reference with the specified type can be resolved.
    /// <param name="type">The type of the reference to check.</param>
    /// <returns>True if the reference can be resolved; otherwise, false.</returns>
    bool IsResolvable(Type type);

    /// Determines if a reference with the specified name and type can be resolved.
    /// <param name="name">The name of the reference to check.</param>
    /// <param name="type">The type of the reference to check.</param>
    /// <returns>True if the reference can be resolved; otherwise, false.</returns>
    bool IsResolvable(string name, Type type);
}
