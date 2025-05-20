namespace Agenix.Api.Validation.Matcher;

/// <summary>
///     Control expression parser for extracting the individual control values from a control expression.
/// </summary>
public interface IControlExpressionParser
{
    /// <summary>
    ///     Some validation matchers can optionally contain one or more control values nested within a control expression
    ///     Matchers implementing this interface should take care of extracting the individual control values from the
    ///     expression. <br />
    ///     For example, expects between 2 and 3 control values(dateFrom, dateTo and optionally datePattern) to be provided.It's
    ///     ControlExpressionParser would be expected to parse the control expression, returning each individual
    ///     control///value for each nested parameter found within the control expression.
    /// </summary>
    /// <param name="controlExpression">the control expression to be parsed</param>
    /// <param name="delimiter">the delimiter to use. When {@literal NULL} the {@link #DEFAULT_DELIMITER} is assumed.</param>
    /// <returns></returns>
    List<string> ExtractControlValues(string controlExpression, char delimiter);
}