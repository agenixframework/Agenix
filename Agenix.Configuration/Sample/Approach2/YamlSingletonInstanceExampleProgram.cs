using Agenix.Configuration.Sample.Models;
using YamlDotNet.Serialization;

namespace Agenix.Configuration.Sample.Approach2;

public class YamlSingletonInstanceExampleProgram
{
    public static void Main(string[] args)
    {
        new YamlConfigurationExample().SetupYamlConfiguration();
    }
}
