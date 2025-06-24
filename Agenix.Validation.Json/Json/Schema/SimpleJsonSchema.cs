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

using Agenix.Api.Common;
using Agenix.Api.Exceptions;
using Agenix.Api.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

#endregion

namespace Agenix.Validation.Json.Json.Schema;

/// <summary>
///     Adapter between the resource reference from the bean configuration and the usable SimpleJsonSchema for validation.
/// </summary>
public class SimpleJsonSchema : InitializingPhase
{
    /// <summary>
    ///     Represents an adapter for handling a JSON schema and its associated resource references,
    ///     enabling usage in validation workflows.
    /// </summary>
    /// <remarks>
    ///     This class integrates with custom resource references and loaded JSON schemas,
    ///     providing a framework to use them within validation functionalities.
    ///     It is designed to be initialized with valid resources and schemas for proper operation.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when required resource or schema objects are null during initialization.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the schema initialization process encounters errors.
    /// </exception>
    public SimpleJsonSchema(IResource json)
    {
        Json = json;
    }

    public SimpleJsonSchema()
    {
    }

    /// <summary>
    ///     The Resource of the JSON schema passed from the file
    /// </summary>
    private IResource Json { get; } = null!;

    /// <summary>
    ///     The parsed JSON schema ready for validation
    /// </summary>
    public JSchema? Schema { get; set; }

    /// <summary>
    ///     Initializes the SimpleJsonSchema instance by loading and parsing
    ///     the JSON schema definition into the Schema property.
    /// </summary>
    /// <remarks>
    ///     This method retrieves the JSON schema from an IResource instance using
    ///     its GetReader() method and processes it using the JSchema.Load method
    ///     provided by the Newtonsoft.Json.Schema library.
    /// </remarks>
    /// <exception cref="IOException">
    ///     Thrown when there is an error in retrieving the JSON schema resource or during schema parsing.
    /// </exception>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when there is an unexpected issue during the loading of the JSON schema.
    /// </exception>
    public void Initialize()
    {
        try
        {
            Schema = JSchema.Load(new JsonTextReader(Json.GetReader()));
        }
        catch (IOException e)
        {
            throw new AgenixSystemException("Failed to load Json schema", e);
        }
    }
}
