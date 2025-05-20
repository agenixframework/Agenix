namespace Agenix.Api.Variable;

/// Represents a collection of global variables accessible in each test case.
/// Allows for managing and storing variables using a builder pattern.
/// /
public class GlobalVariables
{
    /// Stores a collection of key-value pairs representing global variables.
    /// /
    private readonly Dictionary<string, object> _variables;

    public GlobalVariables() : this(new Builder())
    {
    }

    public GlobalVariables(Builder builder)
    {
        _variables = builder.Variables;
    }

    /// Get the global variables.
    /// @return the global variables as a dictionary of key-value pairs.
    /// /
    public Dictionary<string, object> GetVariables()
    {
        return _variables;
    }

    /// Provides a builder for creating and managing global variables.
    /// Supports a fluent interface for defining variables and collections of variables.
    /// Used in conjunction with the GlobalVariables class to construct instances with specified configurations.
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