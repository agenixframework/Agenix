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

#region Imports

using System.Collections.Generic;

#endregion

namespace Agenix.Core.Tests.TypeResolution;

/// <summary>
///     Simple test object used for testing generic objects.
/// </summary>
public class TestGenericObject<T, U>
{
    #region Properties

    public int ID { get; set; }

    public string Name { get; set; } = string.Empty;

    public IList<int> SomeGenericList { get; set; } = new List<int>();

    public IDictionary<string, int> SomeGenericDictionary { get; set; } = new Dictionary<string, int>();

    #endregion

    #region Constructor (s) / Destructor

    public TestGenericObject()
    {
    }

    public TestGenericObject(int id)
    {
        ID = id;
    }

    public TestGenericObject(int id, string name)
    {
        ID = id;
        Name = name;
    }

    #endregion

    #region Methods

    public static List<V> CreateList<V>()
    {
        return new List<V>();
    }

    public TestGenericObject<V, W> CreateInstance<V, W>()
    {
        return new TestGenericObject<V, W>();
    }

    #endregion
}
