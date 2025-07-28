namespace Agenix.Screenplay.Utils;

/// <summary>
///     Provides functionality to replace a placeholder field within a step description
///     with a specified value.
/// </summary>
public class ReplaceField
{
    private readonly string _field;
    private readonly string _stepDescription;

    /// <summary>
    ///     Provides functionality to replace a placeholder field in a step description
    ///     with a specified value.
    /// </summary>
    public ReplaceField(string stepDescription, string field)
    {
        _stepDescription = stepDescription;
        _field = field;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ReplaceFieldBuilder" /> class with the specified step description
    ///     to begin configuring the replacement of a placeholder field.
    /// </summary>
    /// <param name="stepDescription">The step description containing the placeholder to be replaced.</param>
    /// <returns>
    ///     A <see cref="ReplaceFieldBuilder" /> instance for further configuration of the field replacement.
    /// </returns>
    public static ReplaceFieldBuilder In(string stepDescription)
    {
        return new ReplaceFieldBuilder(stepDescription);
    }

    /// <summary>
    ///     Replaces a placeholder field in the step description with a specified value, if applicable.
    /// </summary>
    /// <param name="value">The value to replace the placeholder field with. Can be an object, array, or enumeration.</param>
    /// <returns>
    ///     A string where the placeholder field in the step description is replaced by the specified value,
    ///     or the original step description if no replacement is performed.
    /// </returns>
    public string With(object? value)
    {
        var fieldName = FieldNameFor(_field);
        if (_stepDescription.Contains(fieldName) && value != null && !IsUndefinedValue(value))
        {
            return _stepDescription.Replace(FieldNameFor(_field), StringValueFor(value));
        }

        return _stepDescription;
    }

    private string FieldNameFor(string field)
    {
        return "#" + field;
    }

    private string StringValueFor(object value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is Enum[] enumArray)
        {
            return KeyNamesFor(enumArray);
        }

        if (value.GetType().IsArray)
        {
            var elements = new List<string>();
            var array = (object[])value;

            if (array.Length > 0)
            {
                elements.Add(array[0].ToString());
            }

            return string.Join(",", elements);
        }

        return value.ToString();
    }

    private string KeyNamesFor(Enum[] keyValues)
    {
        var keyNames = keyValues.Select(keyValue => keyValue.ToString()).ToList();
        return string.Join(",", keyNames);
    }

    private bool IsUndefinedValue(object? value)
    {
        // This would need to be adapted based on your specific "UNDEFINED" field value implementation
        // For now, checking for null and common undefined indicators
        if (value == null)
        {
            return true;
        }

        // If you have a specific Fields.FieldValue.UNDEFINED equivalent in C#, check for it here
        var valueString = value.ToString();
        return string.IsNullOrEmpty(valueString) ||
               valueString.Equals("UNDEFINED", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Provides functionality for constructing and configuring a <see cref="ReplaceField" />
    ///     instance to replace a placeholder field in a step description with a specified value.
    /// </summary>
    public class ReplaceFieldBuilder
    {
        private readonly string _stepDescription;

        /// <summary>
        ///     Provides functionality for replacing a placeholder field within a step description
        ///     with a specified value if applicable.
        /// </summary>
        public ReplaceFieldBuilder(string stepDescription)
        {
            _stepDescription = stepDescription;
        }

        /// <summary>
        ///     Specifies the field name to be replaced within the step description.
        /// </summary>
        /// <param name="field">The name of the field to replace within the step description.</param>
        /// <returns>
        ///     A <see cref="ReplaceField" /> instance configured with the given field name and step description.
        /// </returns>
        public ReplaceField TheFieldCalled(string field)
        {
            return new ReplaceField(_stepDescription, field);
        }
    }
}
