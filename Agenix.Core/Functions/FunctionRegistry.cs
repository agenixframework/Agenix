using System.Collections.Generic;
using System.Linq;
using Agenix.Core.Exceptions;

namespace Agenix.Core.Functions
{
    /// <summary>
    ///     Function registry holding all available function libraries.
    /// </summary>
    public class FunctionRegistry
    {
        /// <summary>
        ///     The list of libraries providing custom functions
        /// </summary>
        private List<FunctionLibrary> _functionLibraries = [];

        /// <summary>
        ///     The list of libraries providing custom functions
        /// </summary>
        public List<FunctionLibrary> FunctionLibraries
        {
            get => _functionLibraries;
            set => _functionLibraries = value;
        }

        /// <summary>
        ///     Check if variable expression is a custom function. Expression has to start with one of the registered function
        ///     library prefix.
        /// </summary>
        /// <param name="variableExpression">to be checked</param>
        /// <returns>flag (true/false)</returns>
        public bool IsFunction(string variableExpression)
        {
            return !string.IsNullOrEmpty(variableExpression) &&
                   _functionLibraries.Any(c => variableExpression.StartsWith(c.Prefix));
        }

        /// <summary>
        ///     Get library for function prefix.
        /// </summary>
        /// <param name="functionPrefix"> to be searched for</param>
        /// <returns>The FunctionLibrary instance</returns>
        public FunctionLibrary GetLibraryForPrefix(string functionPrefix)
        {
            var functionLibrary = _functionLibraries.FirstOrDefault(f => f.Prefix.Equals(functionPrefix));

            return functionLibrary ??
                   throw new NoSuchFunctionLibraryException(
                       "Can not find function library for prefix " + functionPrefix);
        }
    }
}