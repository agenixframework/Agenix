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

namespace Agenix.Api;

public class TestSource
{
    /**
     * Test type, name and optional sourceFile
     */
    private readonly string _type;

    public TestSource(string type, string name)
        : this(type, name, null)
    {
    }

    public TestSource(string type, string name, string filePath)
    {
        _type = type;
        Name = name;
        FilePath = filePath;
    }

    public TestSource(Type testClass)
        : this("cs", testClass.Name)
    {
    }

    /**
     * The test source type. Usually one of java, xml, groovy, yaml.
     * @return
     */
    public string Type => _type;

    /**
     * Gets the name.
     * @return
     */
    public string Name { get; }

    /**
     * Optional source file path.
     * @return
     */
    public string FilePath { get; }
}
