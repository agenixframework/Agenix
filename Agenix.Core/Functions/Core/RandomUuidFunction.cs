using System;
using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Creates a random .NET UUID.
/// </summary>
public class RandomUuidFunction : IFunction
{
    /// <summary>
    ///     Generates a new GUID
    /// </summary>
    /// <param name="parameterList"></param>
    /// <param name="testContext"></param>
    /// <returns>new Guid string</returns>
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        return Guid.NewGuid().ToString();
    }
}