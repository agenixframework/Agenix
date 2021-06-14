using System.Collections.Generic;
using System.Xml;
using FleetPay.Core.Exceptions;

namespace FleetPay.Core.Functions.Core
{
    /// <summary>
    ///     Escapes XML fragment with escaped characters for '<', '>'
    /// </summary>
    public class EscapeXmlFunction : IFunction
    {
        public string Execute(List<string> parameterList, TestContext testContext)
        {
            if (parameterList == null || parameterList.Count == 0)
                throw new InvalidFunctionUsageException("Function parameters must not be empty");

            if (parameterList.Count != 1)
                throw new InvalidFunctionUsageException(
                    "Invalid function parameter usage! Expected single parameter but found: " + parameterList.Count);

            var doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerText = parameterList[0];

            return node.InnerXml;
        }
    }
}