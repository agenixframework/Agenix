using System.Collections.Generic;

namespace MPP.Core.Functions
{
    /// <summary>
    /// General function interface.
    /// </summary>
    public interface IFunction
    {
        /// <summary>
        /// Method called on execution.
        /// </summary>
        /// <param name="parameterList">The list of function arguments.</param>
        /// <param name="testContext">The test context</param>
        /// <returns>The function result as string.</returns>
        string Execute(List<string> parameterList, TestContext testContext);
    }
}