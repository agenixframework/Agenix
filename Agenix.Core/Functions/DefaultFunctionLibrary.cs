using Agenix.Core.Functions.Core;
using log4net;

namespace Agenix.Core.Functions;

public class DefaultFunctionLibrary : FunctionLibrary
{
    /**
     * Logger
     */
    private static readonly ILog _log = LogManager.GetLogger(typeof(DefaultFunctionLibrary));

    /**
     * Default constructor adding default function implementations.
     */
    public DefaultFunctionLibrary()
    {
        Name = "agenixFunctionLibrary";

        Members.Add("randomNumber", new RandomNumberFunction());
        Members.Add("RandomUUID", new RandomUuidFunction());
        Members.Add("Concat", new ConcatFunction());
        Members.Add("UpperCase", new UpperCaseFunction());
        Members.Add("EscapeXml", new EscapeXmlFunction());
        Members.Add("CurrentDate", new CurrentDateFunction());
        Members.Add("LowerCase", new LowerCaseFunction());
        Members.Add("RandomString", new RandomStringFunction());
        Members.Add("RandomNumber", new RandomNumberFunction());
        Members.Add("EncodeBase64", new EncodeBase64Function());
        Members.Add("DecodeBase64", new DecodeBase64Function());
        Members.Add("Translate", new TranslateFunction());
        Members.Add("JsonPath", new JsonPathFunction());
    }
}