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

using Agenix.ReqnrollPlugin;
using Reqnroll.Bindings;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;

[assembly: RuntimePlugin(typeof(Plugin))]

namespace Agenix.ReqnrollPlugin;

/// <summary>
///     Registered Reqnroll plugin from configuration file.
/// </summary>
public class Plugin : IRuntimePlugin
{
    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters,
        UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        runtimePluginEvents.CustomizeGlobalDependencies += CustomizeGlobalDependencies;
        runtimePluginEvents.CustomizeScenarioDependencies += CustomizeScenarioDependencies;
    }

    /// <summary>
    ///     Configures the global dependencies within the plugin for runtime execution.
    /// </summary>
    /// <param name="sender">The sender of the event that triggered the customization.</param>
    /// <param name="e">The event arguments containing details about the dependency customization process.</param>
    private void CustomizeGlobalDependencies(object sender, CustomizeGlobalDependenciesEventArgs e)
    {
        // Add this line to ensure your assembly is included
        e.ReqnrollConfiguration.AdditionalStepAssemblies.Add(typeof(AgenixHooks).Assembly.GetName().Name);

        e.ObjectContainer.RegisterTypeAs<SafeBindingInvoker, IAsyncBindingInvoker>();
    }

    private void CustomizeScenarioDependencies(object sender, CustomizeScenarioDependenciesEventArgs e)
    {
        var bindingProvider = new BindingInstanceProvider();

        // Store reference to the container
        bindingProvider.SetObjectContainer(e.ObjectContainer);

        e.ObjectContainer.RegisterInstanceAs(bindingProvider);

        // Load all binding instances if desired
        bindingProvider.GetAllBindingInstances();
    }
}
