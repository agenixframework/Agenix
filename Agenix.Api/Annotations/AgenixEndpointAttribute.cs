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

namespace Agenix.Api.Annotations;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
public class AgenixEndpointAttribute : Attribute
{
    public AgenixEndpointAttribute()
    {
    }

    public AgenixEndpointAttribute(string name, params string[] properties)
    {
        Name = name;
        Properties = ParseProperties(properties);
    }

    /// <summary>
    ///     Endpoint name usually referencing a Spring bean id.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    ///     Endpoint properties.
    /// </summary>
    public AgenixEndpointPropertyAttribute[] Properties { get; set; } = Array.Empty<AgenixEndpointPropertyAttribute>();

    private static AgenixEndpointPropertyAttribute[] ParseProperties(string[] properties)
    {
        var props = new List<AgenixEndpointPropertyAttribute>();

        foreach (var prop in properties)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();

            var parts = prop.Split([':'], 3);
            var name = parts[0];
            var value = parts[1];
            var resultedType = parts.Length > 2
                ? Type.GetType(parts[2]) != null ? Type.GetType(parts[2]) : executingAssembly.GetType(parts[2])
                : typeof(string);
            props.Add(new AgenixEndpointPropertyAttribute(name, value, resultedType));
        }

        return props.ToArray();
    }
}
