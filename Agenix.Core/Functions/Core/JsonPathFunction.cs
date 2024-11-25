using System.Collections.Generic;
using System.Text;
using Agenix.Core.Exceptions;
using Agenix.Core.Json;

namespace Agenix.Core.Functions.Core;

public class JsonPathFunction : IFunction

{
    public string Execute(List<string> parameterList, TestContext context)
    {
        if (parameterList == null || parameterList.Count == 0)
            throw new InvalidFunctionUsageException("Function parameters must not be empty");

        if (parameterList.Count < 2)
            throw new InvalidFunctionUsageException(
                "Missing parameter for function - usage JsonPath('jsonSource','expression')");

        string jsonSource;
        string jsonPathExpression;

        if (parameterList.Count > 2)
        {
            var sb = new StringBuilder();
            sb.Append(parameterList[0]);

            for (var i = 1; i < parameterList.Count - 1; i++) sb.Append(", ").Append(parameterList[i]);

            jsonSource = sb.ToString();
            jsonPathExpression = parameterList[^1];
        }
        else
        {
            jsonSource = parameterList[0];
            jsonPathExpression = parameterList[1];
        }

        return JsonPathUtils.EvaluateAsString(context.ReplaceDynamicContentInString(jsonSource),
            jsonPathExpression);
    }
}