using System;
using System.IO;
using System.Reflection;
using Agenix.Core.Actions;
using Agenix.Core.Exceptions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Actions;

public class LoadAppSettingsActionTest : AbstractNUnitSetUp
{
    [Test]
    public void TestLoadProperties()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var testDirectory = Path.GetDirectoryName(assemblyLocation);
        var filePath = "file:" + testDirectory + @"\ResourcesTest\app.config";

        var loadProperties = new LoadAppSettingsAction.Builder()
            .FilePath(filePath)
            .Build();

        loadProperties.Execute(Context);

        ClassicAssert.IsNotNull(Context.GetVariable("${myVariable}"));
        ClassicAssert.AreEqual("test", Context.GetVariable("${myVariable}"));

        ClassicAssert.IsNotNull(Context.GetVariable("${user}"));
        ClassicAssert.AreEqual("Agenix", Context.GetVariable("${user}"));

        ClassicAssert.IsNotNull(Context.GetVariable("${welcomeText}"));
        ClassicAssert.AreEqual("Hello Agenix!", Context.GetVariable("${welcomeText}"));

        ClassicAssert.IsNotNull(Context.GetVariable("${todayDate}"));
        var expectedDate = "Today is " + DateTime.Now.ToString("yyyy-MM-dd") + "!";
        ClassicAssert.AreEqual(expectedDate, Context.GetVariable("${todayDate}"));
    }

    [Test]
    public void TestUnknownVariableInLoadProperties()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var testDirectory = Path.GetDirectoryName(assemblyLocation);
        var filePath = "file:" + testDirectory + @"\ResourcesTest\app-error.config";

        var loadProperties = new LoadAppSettingsAction.Builder()
            .FilePath(filePath)
            .Build();

        try
        {
            loadProperties.Execute(Context);
        }
        catch (CoreSystemException e)
        {
            ClassicAssert.AreEqual("Unknown variable 'unknownVar'", e.Message);
            return;
        }

        Assert.Fail("Missing exception for unknown variable in config file");
    }
}