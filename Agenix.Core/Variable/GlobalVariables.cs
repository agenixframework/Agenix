using System.Collections.Generic;

/**
 * Global variables valid in each test case.
 */
public class GlobalVariables
{
    /**
     * Variables name value pair map
     */
    private readonly Dictionary<string, object> _variables;

    public GlobalVariables() : this(new Builder())
    {
    }

    public GlobalVariables(Builder builder)
    {
        _variables = builder.Variables;
    }

    /**
     * Get the global variables.
     * @return the variables
     */
    public Dictionary<string, object> GetVariables()
    {
        return _variables;
    }

    /**
     * Fluent builder.
     */
    public class Builder
    {
        public Dictionary<string, object> Variables { get; } = [];

        public Builder WithVariable(string name, object value)
        {
            Variables[name] = value;
            return this;
        }

        public Builder WithVariables(Dictionary<string, object> variables)
        {
            foreach (var item in variables) Variables[item.Key] = item.Value;
            return this;
        }

        public GlobalVariables Build()
        {
            return new GlobalVariables(this);
        }
    }
}