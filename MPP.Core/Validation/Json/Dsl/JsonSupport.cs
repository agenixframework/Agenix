namespace MPP.Core.Validation.Json.Dsl
{
    public class JsonSupport
    {
        /// <summary>
        ///     Static entrance for all Json related Java DSL functionalities.
        /// </summary>
        /// <returns></returns>
        public static JsonSupport Json()
        {
            return new();
        }

        public JsonPathSupport JsonPath()
        {
            return new();
        }

        public sealed class JsonPathSupport
        {
            /// <summary>
            ///     Static entrance for all JsonPath related Java DSL functionalities.
            /// </summary>
            /// <returns></returns>
            public static JsonPathSupport JsonPath()
            {
                return new();
            }

            public JsonPathVariableExtractor.Builder Extract()
            {
                return new();
            }

            public JsonPathValidator.Builder Validate()
            {
                return new();
            }
        }
    }
}