using System.Collections.Generic;
using System.Linq;
using Agenix.Core.Functions;
using Agenix.Core.Variable;
using Agenix.Core.Exceptions;

namespace Agenix.Core.Validation.Matcher
{
    /// <summary>
    ///     Utility class for validation matchers.
    /// </summary>
    public class ValidationMatcherUtils
    {
        /// <summary>
        ///     Prevent class instantiation.
        /// </summary>
        private ValidationMatcherUtils()
        {
        }

        /// <summary>
        ///     This method resolves a custom validationMatcher to its respective result.
        /// </summary>
        /// <param name="fieldName">the name of the field</param>
        /// <param name="fieldValue">the value of the field</param>
        /// <param name="validationMatcherExpression">to evaluate.</param>
        /// <param name="context">the test context</param>
        public static void ResolveValidationMatcher(string fieldName, string fieldValue,
            string validationMatcherExpression, TestContext context)
        {
            var expression =
                VariableUtils.CutOffVariablesPrefix(CutOffValidationMatchersPrefix(validationMatcherExpression));

            if (expression.Equals("Ignore")) expression += "()";

            var bodyStart = expression.IndexOf('(');
            if (bodyStart < 0)
                throw new CoreSystemException(
                    "Illegal syntax for validation matcher expression - missing validation value in '()' function body");

            var prefix = "";
            if (expression.IndexOf(':') > 0 && expression.IndexOf(':') < bodyStart)
                prefix = expression.Substring(0, expression.IndexOf(':') + 1);

            var matcherValue = expression.Substring(bodyStart + 1, expression.IndexOf(')') - bodyStart - 1);
            var matcherName = expression.Substring(prefix.Length, bodyStart - prefix.Length);

            var library = context.ValidationMatcherRegistry.GetLibraryForPrefix(prefix);
            var validationMatcher = library.GetValidationMatcher(matcherName);

            var controlExpressionParser = LookupControlExpressionParser(validationMatcher);
            var parameters =
                controlExpressionParser.ExtractControlValues(matcherValue,
                    DefaultControlExpressionParser.DefaultDelimiter);
            var replacedParams = ReplaceVariablesAndFunctionsInParameters(parameters, context);
            validationMatcher.Validate(fieldName, fieldValue, replacedParams, context);
        }

        private static List<string> ReplaceVariablesAndFunctionsInParameters(IReadOnlyCollection<string> parameters,
            TestContext context)
        {
            var replacedParams = new List<string>(parameters.Count);
            replacedParams.AddRange(parameters
                .Select(param => VariableUtils.ReplaceVariablesInString(param, context, false))
                .Select(parsedVariablesParam => FunctionUtils.ReplaceFunctionsInString(parsedVariablesParam, context)));

            return replacedParams;
        }

        /// <summary>
        ///     Checks if expression is a validation matcher expression.
        /// </summary>
        /// <param name="expression">the expression to check</param>
        /// <returns></returns>
        public static bool IsValidationMatcherExpression(string expression)
        {
            return expression.StartsWith(CoreSettings.ValidationMatcherPrefix) &&
                   expression.EndsWith(CoreSettings.ValidationMatcherSuffix);
        }

        /// <summary>
        ///     Cut off validation matchers prefix and suffix.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static string CutOffValidationMatchersPrefix(string expression)
        {
            if (expression.StartsWith(CoreSettings.ValidationMatcherPrefix) &&
                expression.EndsWith(CoreSettings.ValidationMatcherSuffix))
                return expression.Substring(CoreSettings.ValidationMatcherPrefix.Length,
                    expression.Length - CoreSettings.ValidationMatcherSuffix.Length - 1);

            return expression;
        }

        private static IControlExpressionParser LookupControlExpressionParser(IValidationMatcher validationMatcher)
        {
            if (validationMatcher.GetType() == typeof(IControlExpressionParser))
                return validationMatcher as IControlExpressionParser;

            return new DefaultControlExpressionParser();
        }
    }
}