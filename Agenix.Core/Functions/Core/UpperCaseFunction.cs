using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Returns a given string argument in upper case letters.
/// </summary>
public class UpperCaseFunction : IFunction
{
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count == 0)
            throw new InvalidFunctionUsageException("Function parameters must not be empty");

        return parameterList[0].ToUpper();
    }
}