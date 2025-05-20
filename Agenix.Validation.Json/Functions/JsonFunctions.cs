using Agenix.Api.Context;
using Agenix.Validation.Json.Functions.Core;

namespace Agenix.Validation.Json.Functions
{
    /// <summary>
    /// The <c>JsonFunctions</c> class provides utility methods for processing JSON data.
    /// </summary>
    public sealed class JsonFunctions
    {
        /// <summary>
        /// The <c>JsonFunctions</c> class provides utility methods for handling and processing JSON data.
        /// </summary>
        private JsonFunctions()
        {
        }

        /// <summary>
        /// Executes a JSON path expression on the provided JSON content within the given test context.
        /// </summary>
        /// <param name="content">The JSON content to evaluate the expression against.</param>
        /// <param name="expression">The JSON path expression to execute.</param>
        /// <param name="context">The test context in which the JSON path function is executed.</param>
        /// <returns>The result of executing the JSON path expression as a string.</returns>
        public static string JsonPath(string content, string expression, TestContext context)
        {
            return new JsonPathFunction().Execute(new List<string> { content, expression }, context);
        }
    }
}