using MPP.Core.Functions;

namespace MPP.Core
{
    public class TestContextFactory
    {
        /// <summary>
        /// Gets a new instance of TestContext with default/ core function library initialized.
        /// </summary>
        /// <returns>new instance of TestContext</returns>
        public TestContext GetObject()
        {
            var context = new TestContext();
            var functionConfiguration = new FunctionConfiguration();
            context.FunctionRegistry = functionConfiguration.GetFunctionRegistry();
            context.FunctionRegistry.FunctionLibraries.Add(functionConfiguration.GetCoreFunctionLibrary());

            return context;
        }

        /// <summary>
        /// New instance of TestContextFactory
        /// </summary>
        /// <returns>an instance of TestContextFactory</returns>
        public static TestContextFactory NewInstance()
        {
            return new();
        }
    }
}