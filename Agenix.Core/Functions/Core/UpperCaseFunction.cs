using System.Collections.Generic;
using Agenix.Core;
using Agenix.Core.Functions;
using Agenix.Core.Exceptions;

namespace Agenix.Core.Functions.Core
{
    /// <summary>
    ///     Returns given string argument in upper case letters.
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
}