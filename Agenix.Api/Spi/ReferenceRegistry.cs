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

/**
 * Bind objects to registry for later reference. Objects declared in registry can be injected in various ways (e.g. annotations).
 */
public interface IReferenceRegistry
{
    void Bind(string name, object value);
}

/// ReferenceRegistry is responsible for binding objects to a registry for later reference.
/// Declared objects can be injected in multiple ways, such as through annotations.
/// /
public class ReferenceRegistry : IReferenceRegistry
{
    public void Bind(string name, object value)
    {
        //implement the bind logic here
    }

    /// Get proper bean name for future bind operation on registry.
    /// @param bindAnnotation The annotation containing binding information.
    /// @param defaultName The default name to use if the annotation does not provide a name.
    /// @return The name to use for binding in the registry.
    /// /
    public static string GetName(BindToRegistryAttribute bindAnnotation, string defaultName)
    {
        return !string.IsNullOrEmpty(bindAnnotation.Name) ? bindAnnotation.Name : defaultName;
    }
}
