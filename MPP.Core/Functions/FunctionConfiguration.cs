using MPP.Core.Functions.Core;

namespace MPP.Core.Functions
{
    public class FunctionConfiguration
    {
        private readonly ConcatFunction _concatFunction = new();

        private readonly CurrentDateFunction _currentDateFunction = new();

        private readonly DecodeBase64Function _decodeBase64Function = new();

        private readonly EncodeBase64Function _encodeBase64Function = new();

        private readonly EscapeXmlFunction _escapeXmlFunction = new();

        private readonly JsonPathFunction _jsonPathFunction = new();

        private readonly LowerCaseFunction _lowerCaseFunction = new();

        private readonly RandomNumberFunction _randomNumberFunction = new();

        private readonly RandomStringFunction _randomStringFunction = new();

        private readonly RandomUuidFunction _randomUuidFunction = new();

        private readonly TranslateFunction _translateFunction = new();

        private readonly UpperCaseFunction _upperCaseFunction = new();

        /// <summary>
        ///     Creates a new instance of FunctionRegistry
        /// </summary>
        /// <returns>returns a new instance of FunctionRegistry</returns>
        public FunctionRegistry GetFunctionRegistry()
        {
            return new();
        }

        /// <summary>
        ///     Returns a new core FunctionLibrary initialized with default functions
        /// </summary>
        /// <returns></returns>
        public FunctionLibrary GetCoreFunctionLibrary()
        {
            var coreFunctionLibrary = new FunctionLibrary {Prefix = "core:", Name = "coreFunctionLibrary"};

            coreFunctionLibrary.Members.Add("RandomUUID", _randomUuidFunction);
            coreFunctionLibrary.Members.Add("Concat", _concatFunction);
            coreFunctionLibrary.Members.Add("UpperCase", _upperCaseFunction);
            coreFunctionLibrary.Members.Add("EscapeXml", _escapeXmlFunction);
            coreFunctionLibrary.Members.Add("CurrentDate", _currentDateFunction);
            coreFunctionLibrary.Members.Add("LowerCase", _lowerCaseFunction);
            coreFunctionLibrary.Members.Add("RandomString", _randomStringFunction);
            coreFunctionLibrary.Members.Add("RandomNumber", _randomNumberFunction);
            coreFunctionLibrary.Members.Add("EncodeBase64", _encodeBase64Function);
            coreFunctionLibrary.Members.Add("DecodeBase64", _decodeBase64Function);
            coreFunctionLibrary.Members.Add("Translate", _translateFunction);
            coreFunctionLibrary.Members.Add("JsonPath", _jsonPathFunction);

            return coreFunctionLibrary;
        }
    }
}