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

namespace Agenix.Validation.Xml.Schema;

public class WsdlDefinition
{
    public string TargetNamespace { get; set; }
    public WsdlTypes Types { get; set; }
    public IDictionary<string, string> Namespaces { get; set; }
    public string DocumentBaseUri { get; set; }
    public List<WsdlImport> Imports { get; set; }
}

public class WsdlTypes
{
    public List<object> ExtensibilityElements { get; set; } = new();
}

public class WsdlImport
{
    public string LocationUri { get; set; }
    public string Namespace { get; set; }
}

public class WsdlType
{
    public List<WsdlSchema> Schemas { get; set; } = [];
}

public class WsdlSchema
{
    public string TargetNamespace { get; set; }
    public string ElementFormDefault { get; set; }
    public string AttributeFormDefault { get; set; }
    public List<WsdlSchemaImport> Imports { get; set; } = [];
    public List<WsdlSchemaInclude> Includes { get; set; } = [];
}

public class WsdlSchemaImport
{
    public string Namespace { get; set; }
    public string SchemaLocation { get; set; }
}

public class WsdlSchemaInclude
{
    public string SchemaLocation { get; set; }
}
