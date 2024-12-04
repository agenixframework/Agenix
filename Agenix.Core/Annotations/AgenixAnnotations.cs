using System;
using System.Linq;
using System.Reflection;
using Agenix.Core.Common;
using Agenix.Core.Exceptions;
using Agenix.Core.Spi;
using log4net;

namespace Agenix.Core.Annotations;

/// <summary>
///     Dependency injection support for AgenixFramework, AgenixResource and AgenixEndpoint annotations.
/// </summary>
public abstract class AgenixAnnotations
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(AgenixAnnotations));

    private AgenixAnnotations()
    {
    }

    /// <summary>
    ///     Creates a new Agenix instance and injects all supported components and endpoints into the target object using
    ///     annotations.
    /// </summary>
    /// <param name="target">The target object to which components and endpoints are injected.</param>
    public static void InjectAll(object target)
    {
        // Assuming Agenix class has a static method NewInstance simulating the factory method pattern
        InjectAll(target, Agenix.NewInstance());
    }

    /// <summary>
    ///     Creates new Agenix test context and injects all supported components and endpoints to target object using
    ///     annotations.
    /// </summary>
    /// <param name="target">The object to which components and endpoints will be injected.</param>
    /// <param name="agenixFramework">The framework instance used to retrieve context and perform injection operations.</param>
    public static void InjectAll(object target, Agenix agenixFramework)
    {
        // Call the overload with a new TestContext created from the AgenixContext
        InjectAll(target, agenixFramework, agenixFramework.AgenixContext.CreateTestContext());
    }

    /// <summary>
    ///     Injects all supported components and endpoints into the specified target object using annotations.
    /// </summary>
    /// <param name="target">The target object into which dependencies will be injected.</param>
    /// <param name="agenixFramework">The Agenix framework instance used for dependency injection.</param>
    /// <param name="context">The test context providing additional resources for dependency injection.</param>
    public static void InjectAll(object target, Agenix agenixFramework, TestContext context)
    {
        // Inject the Agenix framework into the target
        InjectAgenixFramework(target, agenixFramework);

        // Retrieve AgenixContext from the framework and inject it
        var agenixContext = agenixFramework.AgenixContext;
        InjectAgenixContext(target, agenixContext);

        // Example of calling configuration parsing
        agenixContext.ParseConfiguration(target);

        // Inject endpoints using context
        InjectEndpoints(target, context);

        // Inject test context
        InjectTestContext(target, context);
    }

    /// <summary>
    ///     Injects endpoint instances into the fields of the specified target object, using the provided test context.
    /// </summary>
    /// <param name="target">The object whose fields will be injected with endpoint instances.</param>
    /// <param name="context">The current test context used to provide endpoint instances for injection.</param>
    public static void InjectEndpoints(object target, TestContext context)
    {
        AgenixEndpointAnnotations.InjectEndpoints(target, context);
    }

    /// <summary>
    ///     Injects an <see cref="Agenix" /> instance into the fields of the test case object that are annotated with
    ///     <see cref="AgenixFrameworkAttribute" />.
    /// </summary>
    /// <param name="testCase">The object whose fields will be injected with the <see cref="Agenix" /> instance.</param>
    /// <param name="agenixFramework">The <see cref="Agenix" /> instance to inject into the test case object's fields.</param>
    /// <exception cref="CoreSystemException">Thrown when the injection into a test case field cannot be completed.</exception>
    public static void InjectAgenixFramework(object testCase, Agenix agenixFramework)
    {
        var testCaseType = testCase.GetType();

        var fields = testCaseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(field => Attribute.IsDefined(field, typeof(AgenixFrameworkAttribute)) &&
                            typeof(Agenix).IsAssignableFrom(field.FieldType));

        foreach (var field in fields)
            try
            {
                Log.Debug($"Injecting Agenix framework instance on test class field '{field.Name}'");
                field.SetValue(testCase, agenixFramework);
            }
            catch (Exception ex)
            {
                throw new CoreSystemException($"Failed to inject Agenix framework for field {field.FieldType}", ex);
            }
    }

    /// <summary>
    ///     Injects an <see cref="AgenixContext" /> instance into the fields of the target object that are annotated with
    ///     <see cref="AgenixResourceAttribute" />.
    /// </summary>
    /// <param name="target">The object whose fields will be injected with the <see cref="AgenixContext" /> instance.</param>
    /// <param name="context">The <see cref="AgenixContext" /> instance to inject into the target object's fields.</param>
    /// <exception cref="CoreSystemException">Thrown when the injection into a target field cannot be completed.</exception>
    public static void InjectAgenixContext(object target, AgenixContext context)
    {
        var targetType = target.GetType();

        var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(field => Attribute.IsDefined(field, typeof(AgenixResourceAttribute)) &&
                            typeof(AgenixContext).IsAssignableFrom(field.FieldType));

        foreach (var field in fields)
            try
            {
                Log.Debug($"Injecting Agenix context instance on test class field '{field.Name}'");
                field.SetValue(target, context);
            }
            catch (Exception ex)
            {
                throw new CoreSystemException($"Failed to inject Agenix context for field {field.FieldType}", ex);
            }
    }

    /// <summary>
    ///     Injects a <see cref="TestContext" /> instance into the fields of the target object that are annotated with
    ///     <see cref="AgenixResourceAttribute" />.
    /// </summary>
    /// <param name="target">The object whose fields will be injected with the <see cref="TestContext" /> instance.</param>
    /// <param name="context">The <see cref="TestContext" /> instance to inject into the target object’s fields.</param>
    /// <exception cref="CoreSystemException">Thrown when the injection into a target field cannot be completed.</exception>
    public static void InjectTestContext(object target, TestContext context)
    {
        var targetType = target.GetType();

        var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(field => Attribute.IsDefined(field, typeof(AgenixResourceAttribute)) &&
                            typeof(TestContext).IsAssignableFrom(field.FieldType));

        foreach (var field in fields)
            try
            {
                Log.Debug($"Injecting test context instance on test class field '{field.Name}'");
                field.SetValue(target, context);
            }
            catch (Exception ex)
            {
                throw new CoreSystemException(
                    $"Not able to provide an Agenix resource injection for type {field.FieldType}", ex);
            }
    }

    /// <summary>
    ///     Injects an instance of <see cref="ITestCaseRunner" /> into the fields of a test class annotated with
    ///     <see cref="AgenixResourceAttribute" />.
    /// </summary>
    /// <param name="target">The target object where the test runner instance will be injected.</param>
    /// <param name="runner">The instance of <see cref="ITestCaseRunner" /> to inject into the target fields.</param>
    /// <exception cref="CoreSystemException">Thrown when the injection into a target field cannot be completed.</exception>
    public static void InjectTestRunner(object target, ITestCaseRunner runner)
    {
        var targetType = target.GetType();

        // Injecting fields with TestCaseRunner
        var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(field => Attribute.IsDefined(field, typeof(AgenixResourceAttribute)) &&
                            typeof(ITestCaseRunner).IsAssignableFrom(field.FieldType));

        foreach (var field in fields)
            try
            {
                Log.Debug($"Injecting test runner instance on test class field '{field.Name}'");
                field.SetValue(target, runner);
            }
            catch (Exception ex)
            {
                throw new CoreSystemException(
                    $"Not able to provide an Agenix resource injection for type {field.FieldType}", ex);
            }

        InjectTestActionRunner(target, runner);
        InjectGherkinTestActionRunner(target, runner);
    }

    /// <summary>
    ///     Injects an instance of <see cref="ITestActionRunner" /> into the fields of a test class annotated with
    ///     <see cref="AgenixResourceAttribute" />.
    /// </summary>
    /// <param name="target">The target object where the test action runner instance will be injected.</param>
    /// <param name="runner">The instance of <see cref="ITestActionRunner" /> to inject into the target fields.</param>
    /// <exception cref="CoreSystemException">Thrown when the injection into a target field cannot be completed.</exception>
    public static void InjectTestActionRunner(object target, ITestActionRunner runner)
    {
        var targetType = target.GetType();

        var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(field => Attribute.IsDefined(field, typeof(AgenixResourceAttribute)) &&
                            typeof(ITestActionRunner).IsAssignableFrom(field.FieldType));

        foreach (var field in fields)
            try
            {
                Log.Debug($"Injecting test action runner instance on test class field '{field.Name}'");
                field.SetValue(target, runner);
            }
            catch (Exception ex)
            {
                throw new CoreSystemException(
                    $"Not able to provide an Agenix resource injection for type {field.FieldType}", ex);
            }
    }

    /// <summary>
    ///     Injects an instance of <see cref="IGherkinTestActionRunner" /> into the fields of a test class annotated with
    ///     <see cref="AgenixResourceAttribute" />.
    /// </summary>
    /// <param name="target">The target object where the Gherkin test action runner instance will be injected.</param>
    /// <param name="runner">The instance of <see cref="IGherkinTestActionRunner" /> to inject into the target fields.</param>
    /// <exception cref="CoreSystemException">Thrown when the injection into a target field cannot be completed.</exception>
    public static void InjectGherkinTestActionRunner(object target, IGherkinTestActionRunner runner)
    {
        var targetType = target.GetType();

        var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(field => Attribute.IsDefined(field, typeof(AgenixResourceAttribute)) &&
                            typeof(IGherkinTestActionRunner).IsAssignableFrom(field.FieldType));

        foreach (var field in fields)
            try
            {
                Log.Debug($"Injecting test action runner instance on test class field '{field.Name}'");
                field.SetValue(target, runner);
            }
            catch (Exception ex)
            {
                throw new CoreSystemException(
                    $"Not able to provide an Agenix resource injection for type {field.FieldType}", ex);
            }
    }

    /// <summary>
    ///     Parses the specified configuration type and binds annotated fields and methods to the reference registry in the
    ///     context.
    /// </summary>
    /// <param name="configType">The type of the configuration to be parsed.</param>
    /// <param name="agenixContext">The context used for binding the parsed configuration.</param>
    /// <exception cref="CoreSystemException">
    ///     Thrown when configuration instance creation fails or when issues occur accessing
    ///     the constructor.
    /// </exception>
    public static void ParseConfiguration(Type configType, AgenixContext agenixContext)
    {
        try
        {
            // Creating an instance of the config type using its default constructor
            var instance = Activator.CreateInstance(configType);
            if (instance == null) throw new CoreSystemException("Instance creation failed for configuration class.");

            // Assuming there is a method similar to parseConfiguration that takes an object
            ParseConfiguration(instance, agenixContext);
        }
        catch (MissingMethodException ex)
        {
            throw new CoreSystemException("Missing default constructor on custom configuration class", ex);
        }
        catch (TargetInvocationException ex)
        {
            throw new CoreSystemException("Exception occurred while invoking the default constructor", ex);
        }
        catch (MemberAccessException ex)
        {
            throw new CoreSystemException("No access to the constructor of the custom configuration class", ex);
        }
    }

    /// <summary>
    ///     Parse given configuration class and bind annotated fields, methods to reference registry.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="agenixContext"></param>
    /// <exception cref="CoreSystemException"></exception>
    public static void ParseConfiguration(object configuration, AgenixContext agenixContext)
    {
        var configType = configuration.GetType();

        // Handle AgenixConfiguration attribute

        if (configType
                .GetCustomAttributes(typeof(AgenixConfigurationAttribute), true)
                .FirstOrDefault() is AgenixConfigurationAttribute agenixConfigurationAttribute)
            foreach (var type in agenixConfigurationAttribute.Classes)
                agenixContext.ParseConfiguration(type);

        // Handle methods with BindToRegistry attribute
        var methods = configType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => Attribute.IsDefined(m, typeof(BindToRegistryAttribute)));

        foreach (var method in methods)
            try
            {
                var name = ReferenceRegistry.GetName(
                    method.GetCustomAttribute<BindToRegistryAttribute>(),
                    method.Name);

                var component = method.Invoke(configuration, null);

                if (component is INamed namedComponent) namedComponent.SetName(name);

                agenixContext.AddComponent(name, component);
            }
            catch (Exception ex) when (ex is TargetInvocationException or MethodAccessException)
            {
                throw new CoreSystemException("Failed to invoke configuration method", ex);
            }

        // Handle fields with BindToRegistry attribute
        var fields = configType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => Attribute.IsDefined(f, typeof(BindToRegistryAttribute)));

        foreach (var field in fields)
            try
            {
                if (field.DeclaringType != null &&
                    (!field.IsPublic || field.IsInitOnly || !field.DeclaringType.IsPublic))
                    field.SetValue(configuration, field.GetValue(configuration)); // Ensure field is accessible

                var name = ReferenceRegistry.GetName(
                    field.GetCustomAttribute<BindToRegistryAttribute>(),
                    field.Name);

                var component = field.GetValue(configuration);

                if (component is INamed namedComponent) namedComponent.SetName(name);

                agenixContext.AddComponent(name, component);
            }
            catch (FieldAccessException ex)
            {
                throw new CoreSystemException("Failed to access configuration field", ex);
            }
    }
}