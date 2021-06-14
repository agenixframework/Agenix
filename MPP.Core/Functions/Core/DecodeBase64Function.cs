using System;
using System.Collections.Generic;
using System.Text;
using FleetPay.Core.Exceptions;

namespace FleetPay.Core.Functions.Core
{
    /// <summary>
    ///     Decodes base64 binary data to a character sequence.
    /// </summary>
    public class DecodeBase64Function : IFunction
    {
        public string Execute(List<string> parameterList, TestContext testContext)
        {
            if (parameterList == null || parameterList.Count == 0)
                throw new InvalidFunctionUsageException("Invalid function parameter usage! Missing parameters!");

            var base64EncodedBytes = Convert.FromBase64String(parameterList[0]);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}