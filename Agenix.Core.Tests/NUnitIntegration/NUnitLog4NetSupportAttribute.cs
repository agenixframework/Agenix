using System;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ITestAction = NUnit.Framework.ITestAction;

namespace Agenix.Core.Tests.NUnitIntegration;

/// An attribute designed to integrate support for log4net with NUnit test methods, classes, interfaces, or assemblies.
/// This attribute enables custom actions to be executed before and after test execution, including log configuration setup,
/// streamlining the logging process for tests run under the NUnit framework.
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class |
                AttributeTargets.Interface | AttributeTargets.Assembly,
    AllowMultiple = true)]
public class NUnitLog4NetSupportAttribute : Attribute, ITestAction
{
    private readonly string _configPath;
    private readonly LogLevel _minimumLevel;

    /// <summary>
    ///     Initializes a new instance of the NUnitLog4NetSupportAttribute with default settings.
    /// </summary>
    public NUnitLog4NetSupportAttribute()
        : this("log4net.config", LogLevel.Debug)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the NUnitLog4NetSupportAttribute with custom settings.
    /// </summary>
    /// <param name="configPath">Path to the log4net configuration file</param>
    /// <param name="minimumLevel">Minimum log level to capture</param>
    public NUnitLog4NetSupportAttribute(string configPath, LogLevel minimumLevel)
    {
        _configPath = configPath;
        _minimumLevel = minimumLevel;
    }

    /// <summary>
    ///     Gets the targets for this test action.
    /// </summary>
    public ActionTargets Targets => ActionTargets.Test | ActionTargets.Suite;

    /// <summary>
    ///     Executes actions before the test execution begins.
    /// </summary>
    /// <param name="test">
    ///     An instance of the test about to be executed, providing context and details of the test case or suite.
    /// </param>
    public void BeforeTest(ITest test)
    {
        if (test.IsSuite)
            ConfigureLogging(_configPath, _minimumLevel);
    }

    /// <summary>
    ///     Performs actions after the test execution is completed.
    /// </summary>
    /// <param name="test">
    ///     The test instance that has been executed, providing context and details of the test case or suite.
    /// </param>
    public void AfterTest(ITest test)
    {
        // No actions needed after test
    }

    /// <summary>
    ///     Configures and initializes the global logging system with the specified settings
    /// </summary>
    /// <param name="configPath">Path to the log4net configuration file</param>
    /// <param name="minimumLevel">Minimum log level to capture</param>
    private static void ConfigureLogging(string configPath, LogLevel minimumLevel)
    {
        // Create a new LoggerFactory
        var factory = LoggerFactory.Create(builder =>
        {
            // Set the minimum log level
            builder.SetMinimumLevel(minimumLevel);

            // Add log4net provider
            builder.AddLog4Net(configPath);
        });

        // Set the LogManager's LoggerFactory property
        LogManager.LoggerFactory = factory;
    }
}
