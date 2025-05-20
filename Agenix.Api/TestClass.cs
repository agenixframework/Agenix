using Agenix.Api.Exceptions;

namespace Agenix.Api;

/// Represents a test class, inheriting from TestSource.
/// This class allows the creation of a test instance with
/// an optional method.
/// /
public class TestClass(Type type, string method) : TestSource(type)
{
    public TestClass(Type type) : this(type, null)
    {
    }

    // Gets the method
    public string Method => method;

    // Read String representation and construct a proper test class
    // instance. Read optional method name information and class 
    // name using format "fully.qualified.class.Name#optionalMethodName()"
    public static TestClass FromString(string testClass)
    {
        try
        {
            string className;
            string methodName = null;

            if (testClass.Contains('#'))
            {
                className = testClass[..testClass.IndexOf('#', StringComparison.Ordinal)];
                methodName = testClass[(testClass.IndexOf('#') + 1)..];
            }
            else
            {
                className = testClass;
            }

            return !string.IsNullOrEmpty(methodName)
                ? new TestClass(System.Type.GetType(className), methodName)
                : new TestClass(System.Type.GetType(className));
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Failed to create test class", e);
        }
    }
}