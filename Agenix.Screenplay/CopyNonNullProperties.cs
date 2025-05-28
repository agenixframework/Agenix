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
