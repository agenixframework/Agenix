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

using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Xml;
using Agenix.Core.Message.Builder;

namespace Agenix.Validation.Xml.Message.Builder;

/// <summary>
///     Provides functionality for building message header data with
///     support for object marshalling. This class extends the capabilities of
///     <see cref="DefaultHeaderDataBuilder" /> by integrating marshaller-specific functionality.
/// </summary>
public class MarshallingHeaderDataBuilder : DefaultHeaderDataBuilder
{
    private readonly IMarshaller _marshaller;
    private readonly string _marshallerName;

    /// <summary>
    ///     Default constructor using just model object.
    /// </summary>
    /// <param name="model">The model object</param>
    public MarshallingHeaderDataBuilder(object model) : base(model)
    {
        _marshaller = null;
        _marshallerName = null;
    }

    /// <summary>
    ///     Default constructor using object marshaller and model object.
    /// </summary>
    /// <param name="model">The model object</param>
    /// <param name="marshaller">The marshaller instance</param>
    public MarshallingHeaderDataBuilder(object model, IMarshaller marshaller) : base(model)
    {
        _marshaller = marshaller;
        _marshallerName = null;
    }

    /// <summary>
    ///     Default constructor using object marshaller name and model object.
    /// </summary>
    /// <param name="model">The model object</param>
    /// <param name="marshallerName">The marshaller name</param>
    public MarshallingHeaderDataBuilder(object model, string marshallerName) : base(model)
    {
        _marshallerName = marshallerName;
        _marshaller = null;
    }

    public override string BuildHeaderData(TestContext context)
    {
        if (HeaderData is null or string)
        {
            return base.BuildHeaderData(context);
        }

        if (_marshaller != null)
        {
            return BuildHeaderData(_marshaller, HeaderData, context);
        }

        if (_marshallerName != null)
        {
            if (context.ReferenceResolver.IsResolvable(_marshallerName))
            {
                var objectMapper = context.ReferenceResolver.Resolve<IMarshaller>(_marshallerName);
                return BuildHeaderData(objectMapper, HeaderData, context);
            }

            throw new AgenixSystemException($"Unable to find proper object marshaller for name '{_marshallerName}'");
        }

        var marshallerMap = context.ReferenceResolver.ResolveAll<IMarshaller>();
        if (marshallerMap.Count == 1)
        {
            return BuildHeaderData(marshallerMap.Values.First(), HeaderData, context);
        }

        throw new AgenixSystemException($"Unable to auto detect object marshaller - " +
                                        $"found {marshallerMap.Count} matching marshaller instances in reference resolver");
    }

    private static string BuildHeaderData(IMarshaller marshaller, object model, TestContext context)
    {
        try
        {
            using var stringWriter = new StringWriter();
            marshaller.Marshal(model, stringWriter);
            var result = stringWriter.ToString();

            return context.ReplaceDynamicContentInString(result);
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Failed to marshal object graph for message header data", e);
        }
    }
}
