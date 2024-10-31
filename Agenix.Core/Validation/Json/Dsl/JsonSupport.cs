namespace Agenix.Core.Validation.Json.Dsl;

public class JsonSupport
{
    /// <summary>
    ///     Static entrance for all Json related Java DSL functionalities.
    /// </summary>
    /// <returns></returns>
    public static JsonSupport Json()
    {
        return new JsonSupport();
    }

    public JsonPathSupport JsonPath()
    {
        return new JsonPathSupport();
    }

    public sealed class JsonPathSupport
    {
        /// <summary>
        ///     Static entrance for all JsonPath related Java DSL functionalities.
        /// </summary>
        /// <returns></returns>
        public static JsonPathSupport JsonPath()
        {
            return new JsonPathSupport();
        }

        public JsonPathVariableExtractor.Builder Extract()
        {
            return new JsonPathVariableExtractor.Builder();
        }

        public JsonPathValidator.Builder Validate()
        {
            return new JsonPathValidator.Builder();
        }
    }
}