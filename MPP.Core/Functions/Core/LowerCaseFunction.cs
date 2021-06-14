using System.Collections.Generic;
using FleetPay.Core.Exceptions;

namespace FleetPay.Core.Functions.Core
{
    /// <summary>
    ///     Function returns given string argument in lower case.
    /// </summary>
    public class LowerCaseFunction : IFunction
    {
        public string Execute(List<string> parameterList, TestContext testContext)
        {
            if (parameterList == null || parameterList.Count == 0)
                throw new InvalidFunctionUsageException("Function parameters must not be empty");

            return parameterList[0].ToLower();
        }
    }
}