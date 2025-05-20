using System;
using System.IO;
using System.Reflection;
using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Actions;

public class LoadAppSettingsActionTest : AbstractNUnitSetUp
{
    [Test]
    public void TestLoadProperties()
    {
        var resourceName =
            $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest/app.config";

        var loadProperties = new LoadAppSettingsAction.Builder()
            .ResourceName(resourceName)
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
        var resourceName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ResourcesTest", "app-error.config");
        resourceName = $"file://{resourceName.Replace("\\", "/")}";

        var loadProperties = new LoadAppSettingsAction.Builder()
            .ResourceName(resourceName)
            .Build();

        try
        {
            loadProperties.Execute(Context);
        }
        catch (AgenixSystemException e)
        {
            ClassicAssert.AreEqual("Unknown variable 'unknownVar'", e.Message);
            return;
        }

        Assert.Fail("Missing exception for unknown variable in config file");
    }
}