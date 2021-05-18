using System.Collections.Generic;
using MPP.Core.Exceptions;

namespace MPP.Core.Validation.Matcher
{
    /// <summary>
    ///     Default implementation of control expression parser.
    /// </summary>
    public class DefaultControlExpressionParser : IControlExpressionParser
    {
        public const char DefaultDelimiter = '\'';

        public List<string> ExtractControlValues(string controlExpression, char delimiter)
        {
            var useDelimiter = delimiter;
            var extractedParameters = new List<string>();

            if (string.IsNullOrEmpty(controlExpression)) return extractedParameters;

            ExtractParameters(controlExpression, useDelimiter, extractedParameters, 0);
            if (extractedParameters.Count == 0)
                // if the controlExpression has text but no parameters were extracted, then assume that
                // the controlExpression itself is the only parameter
                extractedParameters.Add(controlExpression);

            return extractedParameters;
        }

        private void ExtractParameters(string controlExp, char delimiter, ICollection<string> extractedParameters,
            int searchFrom)
        {
            var startParameter = controlExp.IndexOf(delimiter, searchFrom);

            if (startParameter <= -1) return;
            var endParameter = controlExp.IndexOf(delimiter, startParameter + 1);

            var isEnd = false;

            while (!isEnd && endParameter > 0 && endParameter < controlExp.Length - 2)
                if (controlExp[endParameter + 1] == ',' || controlExp[endParameter + 1] == ')')
                    isEnd = true;
                else
                    endParameter = controlExp.IndexOf(delimiter, endParameter + 1);

            if (endParameter > -1)
            {
                var extractedParameter = controlExp.Substring(startParameter + 1, endParameter - startParameter - 1);
                extractedParameters.Add(extractedParameter);
                var commaSeparator = controlExp.IndexOf(',', endParameter);
                if (commaSeparator > -1)
                    ExtractParameters(controlExp, delimiter, extractedParameters, endParameter + 1);
            }
            else
            {
                throw new CoreSystemException(
                    $"No matching delimiter ({delimiter}) found after position '{endParameter}' in control expression: {controlExp}");
            }
        }
    }
}