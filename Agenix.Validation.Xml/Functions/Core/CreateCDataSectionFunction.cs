using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Validation.Xml.Functions.Core;

/// <summary>
/// Adds XML CDATA section tags to parameter value. This is extremely useful when having
/// CDATA sections in the message payload. Agenix test case itself also uses CDATA sections and
/// nested CDATA sections are not allowed. This function adds the CDATA section tags
/// at runtime.
/// </summary>
public class CreateCDataSectionFunction : IFunction
{
    /// <summary>
    /// CDATA section tags
    /// </summary>
    private static readonly string CDataStart = "<![CDATA[";
    private static readonly string CDataEnd = "]]>";

    public string Execute(List<string> parameterList, TestContext context)
    {
        if (parameterList is not { Count: 1 })
        {
            throw new InvalidFunctionUsageException("Invalid function parameter usage - missing parameter value!");
        }

        return CDataStart + parameterList[0] + CDataEnd;
    }
}
