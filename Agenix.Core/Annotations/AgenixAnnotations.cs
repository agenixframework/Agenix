#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
//
// Copyright (c) 2025 Agenix
//
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.Api.Common;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Annotations;

/// <summary>
///     Dependency injection support for AgenixFramework, AgenixResource and AgenixEndpoint annotations.
/// </summary>
public abstract class AgenixAnnotations
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AgenixAnnotations));

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
        // Assuming Agenix class has a static method, NewInstance simulating the factory method pattern
        InjectAll(target, Agenix.NewInstance());
    }

    /// <summary>
    ///     Creates a new Agenix test context and injects all supported components and endpoints to target object using
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
    /// <exception cref="AgenixSystemException">Thrown when the injection into a test case field cannot be completed.</exception>
    public static void InjectAgenixFramework(object testCase, Agenix agenixFramework)
    {
        var testCaseType = testCase.GetType();

        var fields = testCaseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(field => Attribute.IsDefined(field, typeof(AgenixFrameworkAttribute)) &&
                            typeof(Agenix).IsAssignableFrom(field.FieldType));

        foreach (var field in fields)
        {
            try
            {
                Log.LogDebug("Injecting Agenix framework instance on test class field '{FieldName}'", field.Name);
                field.SetValue(testCase, agenixFramework);
            }
            catch (Exception ex)
            {
                throw new AgenixSystemException($"Failed to inject Agenix framework for field {field.FieldType}", ex);
            }
        }
    }

    /// <summary>
    ///     Injects an <see cref="AgenixContext" /> instance into the fields of the target object that are annotated with
    ///     <see cref="AgenixResourceAttribute" />.
    /// </summary>
    /// <param name="target">The object whose fields will be injected with the <see cref="AgenixContext" /> instance.</param>
    /// <param name="context">The <see cref="AgenixContext" /> instance to inject into the target object's fields.</param>
    /// <exception cref="AgenixSystemException">Thrown when the injection into a target field cannot be completed.</exception>
    public static void InjectAgenixContext(object target, AgenixContext context)
    {
        var targetType = target.GetType();

        var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(field => Attribute.IsDefined(field, typeof(AgenixResourceAttribute)) &&
                            typeof(AgenixContext).IsAssignableFrom(field.FieldType));

        foreach (var field in fields)
        {
            try
            {
                Log.LogDebug("Injecting Agenix context instance on test class field '{FieldName}'", field.Name);
                field.SetValue(target, context);
            }
            catch (Exception ex)
            {
                throw new AgenixSystemException($"Failed to inject Agenix context for field {field.FieldType}", ex);
            }
        }
    }

    /// <summary>
    ///     Injects a <see cref="TestContext" /> instance into the fields of the target object that are annotated with
    ///     <see cref="AgenixResourceAttribute" />.
    /// </summary>
    /// <param name="target">The object whose fields will be injected with the <see cref="TestContext" /> instance.</param>
    /// <param name="context">The <see cref="TestContext" /> instance to inject into the target object’s fields.</param>
    /// <exception cref="AgenixSystemException">Thrown when the injection into a target field cannot be completed.</exception>
    public static void InjectTestContext(object target, TestContext context)
    {
        var targetType = target.GetType();

        var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(field => Attribute.IsDefined(field, typeof(AgenixResourceAttribute)) &&
                            typeof(TestContext).IsAssignableFrom(field.FieldType));

        foreach (var field in fields)
        {
            try
            {
                Log.LogDebug("Injecting test context instance on test class field '{FieldName}'", field.Name);
                field.SetValue(target, context);
            }
            catch (Exception ex)
            {
                throw new AgenixSystemException(
                    $"Not able to provide an Agenix resource injection for type {field.FieldType}", ex);
            }
        }
    }

    /// <summary>
    ///     Injects an instance of <see cref="IAsyncTestCaseRunner" /> into the fields of a test class annotated with
    ///     <see cref="AgenixResourceAttribute" />.
    /// </summary>
    /// <param name="target">The target object where the test runner instance will be injected.</param>
    /// <param name="runner">The instance of <see cref="IAsyncTestCaseRunner" /> to inject into the target fields.</param>
    /// <exception cref="AgenixSystemException">Thrown when the injection into a target field cannot be completed.</exception>
    public static void InjectTestRunner(object target, IAsyncTestCaseRunner runner)
    {
        var targetType = target.GetType();

        // Injecting fields with TestCaseRunner
        var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(field => Attribute.IsDefined(field, typeof(AgenixResourceAttribute)) &&
                            typeof(IAsyncTestCaseRunner).IsAssignableFrom(field.FieldType));

        foreach (var field in fields)
        {
            try
            {
                Log.LogDebug("Injecting test runner instance on test class field '{FieldName}'", field.Name);
                field.SetValue(target, runner);
            }
            catch (Exception ex)
            {
                throw new AgenixSystemException(
                    $"Not able to provide an Agenix resource injection for type {field.FieldType}", ex);
            }
        }

        InjectTestActionRunner(target, runner);
        InjectGherkinTestActionRunner(target, runner);
    }

    /// <summary>
    ///     Injects an instance of <see cref="IAsyncTestCaseRunner" /> into the fields of a test class annotated with
    ///     <see cref="AgenixResourceAttribute" />.
    /// </summary>
    /// <param name="target">The target object where the test action runner instance will be injected.</param>
    /// <param name="runner">The instance of <see cref="IAsyncTestCaseRunner" /> to inject into the target fields.</param>
    /// <exception cref="AgenixSystemException">Thrown when the injection into a target field cannot be completed.</exception>
    public static void InjectTestActionRunner(object target, IAsyncTestCaseRunner runner)
    {
        var targetType = target.GetType();

        var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(field => Attribute.IsDefined(field, typeof(AgenixResourceAttribute)) &&
                            typeof(IAsyncTestActionRunner).IsAssignableFrom(field.FieldType));

        foreach (var field in fields)
        {
            try
            {
                Log.LogDebug("Injecting test action runner instance on test class field '{FieldName}'", field.Name);
                field.SetValue(target, runner);
            }
            catch (Exception ex)
            {
                throw new AgenixSystemException(
                    $"Not able to provide an Agenix resource injection for type {field.FieldType}", ex);
            }
        }
    }

    /// <summary>
    ///     Injects an instance of <see cref="IGherkinAsyncTestActionRunner" /> into the fields of a test class annotated with
    ///     <see cref="AgenixResourceAttribute" />.
    /// </summary>
    /// <param name="target">The target object where the Gherkin test action runner instance will be injected.</param>
    /// <param name="runner">The instance of <see cref="IGherkinAsyncTestActionRunner" /> to inject into the target fields.</param>
    /// <exception cref="AgenixSystemException">Thrown when the injection into a target field cannot be completed.</exception>
    public static void InjectGherkinTestActionRunner(object target, IGherkinAsyncTestActionRunner runner)
    {
        var targetType = target.GetType();

        var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(field => Attribute.IsDefined(field, typeof(AgenixResourceAttribute)) &&
                            typeof(IGherkinAsyncTestActionRunner).IsAssignableFrom(field.FieldType));

        foreach (var field in fields)
        {
            try
            {
                Log.LogDebug("Injecting test action runner instance on test class field '{FieldName}'", field.Name);
                field.SetValue(target, runner);
            }
            catch (Exception ex)
            {
                throw new AgenixSystemException(
                    $"Not able to provide an Agenix resource injection for type {field.FieldType}", ex);
            }
        }
    }

    /// <summary>
    ///     Parses the specified configuration type and binds annotated fields and methods to the reference registry in the
    ///     context.
    /// </summary>
    /// <param name="configType">The type of the configuration to be parsed.</param>
    /// <param name="agenixContext">The context used for binding the parsed configuration.</param>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when configuration instance creation fails or when issues occur accessing
    ///     the constructor.
    /// </exception>
    public static void ParseConfiguration(Type configType, AgenixContext agenixContext)
    {
        try
        {
            // Creating an instance of the config type using its default constructor
            var instance = Activator.CreateInstance(configType);
            if (instance == null)
            {
                throw new AgenixSystemException("Instance creation failed for configuration class.");
            }

            // Assuming there is a method similar to parseConfiguration that takes an object
            ParseConfiguration(instance, agenixContext);
        }
        catch (MissingMethodException ex)
        {
            throw new AgenixSystemException("Missing default constructor on custom configuration class", ex);
        }
        catch (TargetInvocationException ex)
        {
            throw new AgenixSystemException("Exception occurred while invoking the default constructor", ex);
        }
        catch (MemberAccessException ex)
        {
            throw new AgenixSystemException("No access to the constructor of the custom configuration class", ex);
        }
    }

    /// <summary>
    ///     Parse given configuration class and bind annotated fields, methods to reference registry.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="agenixContext"></param>
    /// <exception cref="AgenixSystemException"></exception>
    public static void ParseConfiguration(object configuration, AgenixContext agenixContext)
    {
        var configType = configuration.GetType();

        ProcessAgenixConfigurationAttribute(configType, agenixContext);
        ProcessMethodsWithBindToRegistry(configuration, configType, agenixContext);
        ProcessFieldsWithBindToRegistry(configuration, configType, agenixContext);
    }

    private static void ProcessAgenixConfigurationAttribute(Type configType, AgenixContext agenixContext)
    {
        var agenixConfigAttr = configType.GetCustomAttribute<AgenixConfigurationAttribute>();
        if (agenixConfigAttr?.Classes != null)
        {
            foreach (var type in agenixConfigAttr.Classes)
            {
                agenixContext.ParseConfiguration(type);
            }
        }
    }

    private static void ProcessMethodsWithBindToRegistry(object configuration, Type configType,
        AgenixContext agenixContext)
    {
        var methods = GetMembersWithAttribute<MethodInfo, BindToRegistryAttribute>(
            configType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));

        foreach (var method in methods)
        {
            try
            {
                var component = method.Invoke(configuration, null);
                AddComponentToContext(component, method.GetCustomAttribute<BindToRegistryAttribute>(), method.Name,
                    agenixContext);
            }
            catch (Exception ex) when (ex is TargetInvocationException or MethodAccessException)
            {
                throw new AgenixSystemException("Failed to invoke configuration method", ex);
            }
        }
    }

    private static void ProcessFieldsWithBindToRegistry(object configuration, Type configType,
        AgenixContext agenixContext)
    {
        var fields = GetMembersWithAttribute<FieldInfo, BindToRegistryAttribute>(
            configType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));

        foreach (var field in fields)
        {
            try
            {
                EnsureFieldAccessible(field, configuration);
                var component = field.GetValue(configuration);
                AddComponentToContext(component, field.GetCustomAttribute<BindToRegistryAttribute>(), field.Name,
                    agenixContext);
            }
            catch (FieldAccessException ex)
            {
                throw new AgenixSystemException("Failed to access configuration field", ex);
            }
        }
    }

    private static IEnumerable<T> GetMembersWithAttribute<T, TAttribute>(T[] members)
        where T : MemberInfo
        where TAttribute : Attribute
    {
        return members.Where(m => Attribute.IsDefined(m, typeof(TAttribute)));
    }

    private static void AddComponentToContext(object component, BindToRegistryAttribute attribute, string memberName,
        AgenixContext agenixContext)
    {
        var name = ReferenceRegistry.GetName(attribute, memberName);

        if (component is INamed namedComponent)
        {
            namedComponent.SetName(name);
        }

        agenixContext.AddComponent(name, component);
    }

    private static void EnsureFieldAccessible(FieldInfo field, object configuration)
    {
        if (field.DeclaringType != null &&
            (!field.IsPublic || field.IsInitOnly || !field.DeclaringType.IsPublic))
        {
            field.SetValue(configuration, field.GetValue(configuration));
        }
    }
}
