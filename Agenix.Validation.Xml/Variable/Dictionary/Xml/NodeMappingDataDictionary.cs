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

using System.Xml;
using Agenix.Api.Common;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Api.Variable.Dictionary;
using Agenix.Validation.Xml.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Variable.Dictionary.Xml;

/// <summary>
///     Very basic data dictionary that holds a list of mappings for message elements. Mapping key is the element path
///     inside
///     the XML structure. The mapping value is set as a new element
///     value where test variables are supported in value expressions.
/// </summary>
public class NodeMappingDataDictionary : AbstractXmlDataDictionary, InitializingPhase
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(NodeMappingDataDictionary));

    public new void Initialize()
    {
        base.Initialize();
    }

    public override T Translate<T>(XmlNode node, T value, TestContext context)
    {
        var nodePath = XmlUtils.GetNodesPathName(node);

        switch (PathMappingStrategy)
        {
            case PathMappingStrategy.EXACT:
                {
                    if (!Mappings.TryGetValue(nodePath, out var mapping))
                    {
                        return value;
                    }

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.LogDebug("Data dictionary setting element '{NodePath}' with value: {Value}",
                            nodePath, mapping);
                    }

                    return ConvertIfNecessary(Mappings[nodePath], value, context);
                }
            case PathMappingStrategy.ENDS_WITH:
                {
                    foreach (var entry in Mappings.Where(entry => nodePath.EndsWith(entry.Key)))
                    {
                        if (Log.IsEnabled(LogLevel.Debug))
                        {
                            Log.LogDebug("Data dictionary setting element '{NodePath}' with value: {Value}",
                                nodePath, entry.Value);
                        }

                        return ConvertIfNecessary(entry.Value, value, context);
                    }

                    break;
                }
            case PathMappingStrategy.STARTS_WITH:
                {
                    foreach (var entry in Mappings.Where(entry => nodePath.StartsWith(entry.Key)))
                    {
                        if (Log.IsEnabled(LogLevel.Debug))
                        {
                            Log.LogDebug("Data dictionary setting element '{NodePath}' with value: {Value}",
                                nodePath, entry.Value);
                        }

                        return ConvertIfNecessary(entry.Value, value, context);
                    }

                    break;
                }
        }

        return value;
    }
}
