namespace Agenix.Api;

public class TestSource
{
    /**
     * Test type, name and optional sourceFile
     */
    private readonly string _type;

    public TestSource(string type, string name)
        : this(type, name, null)
    {
    }

    public TestSource(string type, string name, string filePath)
    {
        _type = type;
        Name = name;
        FilePath = filePath;
    }

    public TestSource(Type testClass)
        : this("cs", testClass.Name)
    {
    }

    /**
     * The test source type. Usually one of java, xml, groovy, yaml.
     * @return
     */
    public string Type => _type;

    /**
     * Gets the name.
     * @return
     */
    public string Name { get; }

    /**
     * Optional source file path.
     * @return
     */
    public string FilePath { get; }
}