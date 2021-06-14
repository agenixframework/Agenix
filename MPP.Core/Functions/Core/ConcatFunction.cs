using System.Collections.Generic;
using FleetPay.Core.Exceptions;

namespace FleetPay.Core.Functions.Core
{
    /// <summary>
    ///     Function concatenating multiple tokens to a single string. Tokens can be either static string values or dynamic
    ///     variables or functions.
    /// </summary>
    public class ConcatFunction : IFunction
    {
        public string Execute(List<string> parameterList, TestContext testContext)
        {
            if (parameterList == null || parameterList.Count == 0)
                throw new InvalidFunctionUsageException("Function parameters must not be empty");

            return string.Join("", parameterList);
        }
    }
}