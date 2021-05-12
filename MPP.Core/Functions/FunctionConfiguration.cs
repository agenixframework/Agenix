using MPP.Core.Functions.Core;

namespace MPP.Core.Functions
{
    public class FunctionConfiguration
    {
        private readonly ConcatFunction _concatFunction = new();

        private readonly CurrentDateFunction _currentDateFunction = new();

        private readonly EscapeXmlFunction _escapeXmlFunction = new();

        private readonly RandomUuidFunction _randomUuidFunction = new();

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

            return coreFunctionLibrary;
        }
    }
}