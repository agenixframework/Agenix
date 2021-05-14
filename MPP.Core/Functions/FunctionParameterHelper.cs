using System.Collections.Generic;
using System.Linq;

namespace MPP.Core.Functions
{
    /// <summary>
    ///     Helper class parsing a parameter string and converting the tokens to a parameter list.
    /// </summary>
    public sealed class FunctionParameterHelper
    {
        /// <summary>
        ///     Prevent class instantiation.
        /// </summary>
        private FunctionParameterHelper()
        {
        }

        /// <summary>
        ///     Convert a parameter string to a list of parameters.
        /// </summary>
        /// <param name='parameterString'>Comma separated parameter string.</param>
        /// <returns>The list of parameters</returns>
        public static List<string> GetParameterList(string parameterString)
        {
            var stringsSplit = parameterString.Split(",");

            IList<string> parameterList = stringsSplit
                .Select(stringSplit => CutOffSingleQuotes(stringSplit.Trim()))
                .ToList();

            var postProcessedList = new List<string>();

            for (var i = 0; i < parameterList.Count; i++)
            {
                var next = i + 1;

                var processed = parameterList[i];

                if (processed.StartsWith("'") && !processed.EndsWith("'"))
                    while (next < parameterList.Count)
                    {
                        if (parameterString.Contains(processed + ", " + parameterList[next]))
                            processed += ", " + parameterList[next];
                        else if (parameterString.Contains(processed + ", " + parameterList[next]))
                            processed += "," + parameterList[next];
                        else if (parameterString.Contains(processed + " , " + parameterList[next]))
                            processed += " , " + parameterList[next];
                        else
                            processed += parameterList[next];

                        i++;
                        if (parameterList[next].EndsWith("'")) break;

                        next++;
                    }

                postProcessedList.Add(CutOffSingleQuotes(processed));
            }

            return postProcessedList;
        }

        private static string CutOffSingleQuotes(string param)
        {
            if (param.Equals("'")) return "";

            if (param.Length > 1 && param[0] == '\'' && param[^1] == '\'') return param.Substring(1, param.Length - 2);

            return param;
        }
    }
}