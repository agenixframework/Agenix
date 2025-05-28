namespace Agenix.Screenplay;

/// <summary>
/// Represents a utility class used to set custom field values for an instance of an AnonymousPerformable.
/// </summary>
/// <typeparam name="T">The type of AnonymousPerformable that is being manipulated.</typeparam>
public class AnonymousPerformableFieldSetter<T>(T anonymousPerformable, string fieldName)
    where T : AnonymousPerformable
{
    public T Of(object fieldValue)
    {
        anonymousPerformable.SetFieldValue(fieldName, fieldValue);
        return anonymousPerformable;
    }
}
