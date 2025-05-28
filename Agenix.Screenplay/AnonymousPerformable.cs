namespace Agenix.Screenplay;

/// <summary>
/// A performable task that can be created anonymously with a title and a list of steps.
/// </summary>
public class AnonymousPerformable : IPerformable, IHasCustomFieldValues
{
    private readonly string _title;
    private readonly Dictionary<string, object> _fieldValues = new();
    private readonly List<IPerformable> _steps;

    public AnonymousPerformable()
    {
    }

    public AnonymousPerformable(string title, List<IPerformable> steps)
    {
        _title = title;
        _steps = steps;
    }
    
    public void PerformAs<T>(T actor) where T : Actor
    {
        actor.AttemptsTo(_steps.ToArray());
    }

    public void SetFieldValue(string fieldName, object fieldValue)
    {
        _fieldValues[fieldName] = fieldValue;
    }

    public override string ToString()
    {
        return _title;
    }

    public IDictionary<string, object> CustomFieldValues => 
        new Dictionary<string, object>(_fieldValues);
}
