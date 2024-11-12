using System.Collections.Generic;

namespace Agenix.Core.Builder;

public interface IWithExpressions<out TB>
{
    /// <summary>
    ///     Sets the expressions to evaluate. Keys are expressions that should be evaluated and values are target
    ///     variable names that are stored in the test context with the evaluated result as variable value.
    /// </summary>
    /// <param name="expressions">A dictionary of expressions and their corresponding target variable names</param>
    /// <returns>The type that implements this interface</returns>
    TB Expressions(IDictionary<string, object> expressions);

    /// <summary>
    ///     Adds an expression that gets evaluated. The evaluation result is stored in the test context as a variable
    ///     with the given variable name.
    /// </summary>
    /// <param name="expression">The expression to be evaluated</param>
    /// <param name="value">The target variable name</param>
    /// <returns>The type that implements this interface</returns>
    TB Expression(string expression, object value);
}