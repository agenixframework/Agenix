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
using System.Runtime.CompilerServices;

namespace Agenix.Screenplay;

public class CopyNonNullProperties
{
    private readonly object source;

    private CopyNonNullProperties(object source)
    {
        this.source = source;
    }

    public static CopyNonNullProperties From(object task)
    {
        return new CopyNonNullProperties(task);
    }

    public void To(object target)
    {
        GetFields(source.GetType())
            .Where(field => !field.IsDefined(typeof(CompilerGeneratedAttribute), false))
            .Where(field => !field.IsStatic)
            .ToList()
            .ForEach(field => CopyFieldValue(field, source, target));
    }

    public static List<FieldInfo> GetFields(Type type)
    {
        var fields = new List<FieldInfo>();
        var typeToInspect = type;

        while (typeToInspect != null)
        {
            fields.AddRange(typeToInspect.GetFields(BindingFlags.Instance |
                                                    BindingFlags.NonPublic |
                                                    BindingFlags.Public |
                                                    BindingFlags.DeclaredOnly));
            typeToInspect = typeToInspect.BaseType;
        }

        return fields;
    }

    private void CopyFieldValue(FieldInfo field, object source, object target)
    {
        try
        {
            var sourceValue = field.GetValue(source);
            if (sourceValue != null)
            {
                field.SetValue(target, sourceValue);
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Failed to copy field value", ex);
        }
    }
}
