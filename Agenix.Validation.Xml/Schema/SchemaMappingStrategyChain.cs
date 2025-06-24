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

namespace Agenix.Validation.Xml.Schema;

/// <summary>
///     Special schema mapping strategy delegating to several other strategies in
///     a mapping chain. The first mapping strategy finding a proper schema wins.
/// </summary>
public class SchemaMappingStrategyChain : IXsdSchemaMappingStrategy
{
    /// <summary>
    ///     List of strategies to use in this chain
    /// </summary>
    private List<IXsdSchemaMappingStrategy> _strategies = [];

    /// <summary>
    ///     Gets the number of strategies in the chain.
    /// </summary>
    public int StrategyCount => _strategies.Count;

    /// <summary>
    ///     Gets the schema for given namespace or root element name.
    /// </summary>
    /// <param name="schemas">List of available schemas.</param>
    /// <param name="document">Document instance to validate.</param>
    /// <returns>The matching schema or null if no match found.</returns>
    public IXsdSchema? GetSchema(List<IXsdSchema> schemas, XmlDocument document)
    {
        IXsdSchema? schema = null;

        foreach (var strategy in _strategies)
        {
            schema = strategy.GetSchema(schemas, document);

            if (schema != null)
            {
                return schema;
            }
        }

        return schema;
    }

    /// <summary>
    ///     Sets the strategies.
    /// </summary>
    /// <param name="strategies">The strategies to set</param>
    public void SetStrategies(List<IXsdSchemaMappingStrategy> strategies)
    {
        _strategies = strategies ?? [];
    }

    /// <summary>
    ///     Gets the list of strategies.
    /// </summary>
    /// <returns>The list of strategies</returns>
    public List<IXsdSchemaMappingStrategy> GetStrategies()
    {
        return _strategies;
    }

    /// <summary>
    ///     Adds a strategy to the chain.
    /// </summary>
    /// <param name="strategy">The strategy to add</param>
    public void AddStrategy(IXsdSchemaMappingStrategy strategy)
    {
        if (strategy != null)
        {
            _strategies.Add(strategy);
        }
    }

    /// <summary>
    ///     Removes a strategy from the chain.
    /// </summary>
    /// <param name="strategy">The strategy to remove</param>
    /// <returns>True if the strategy was removed, false otherwise</returns>
    public bool RemoveStrategy(IXsdSchemaMappingStrategy strategy)
    {
        return _strategies.Remove(strategy);
    }

    /// <summary>
    ///     Clears all strategies from the chain.
    /// </summary>
    public void ClearStrategies()
    {
        _strategies.Clear();
    }
}
