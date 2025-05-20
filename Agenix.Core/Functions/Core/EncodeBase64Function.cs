using System;
using System.Collections.Generic;
using System.Text;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Encodes a character sequence to base64 binary using given charset
/// </summary>
public class EncodeBase64Function : IFunction
{
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count == 0)
            throw new InvalidFunctionUsageException("Invalid function parameter usage! Missing parameters!");

        var plainTextBytes = Encoding.UTF8.GetBytes(parameterList[0]);
        return Convert.ToBase64String(plainTextBytes);
    }
}