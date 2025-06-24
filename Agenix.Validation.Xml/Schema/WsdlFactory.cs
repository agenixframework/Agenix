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

/// <summary>
///     Interface for a factory that creates instances of WSDL (Web Services Description Language) readers.
///     Provides a mechanism for obtaining new WSDL reader instances and allows for centralized
///     management of how these instances are created.
/// </summary>
/// <remarks>
///     The <see cref="IWsdlFactory" /> interface defines the contract for a factory that
///     can create WSDL reader instances. Implementations of this interface, such as
///     <see cref="WsdlFactory" />, may encapsulate the logic for constructing different
///     types of WSDL readers, ensuring modularity and ease of use.
/// </remarks>
public interface IWsdlFactory
{
    IWsdlReader NewWsdlReader();

    static IWsdlFactory NewInstance()
    {
        return new WsdlFactory();
    }
}

/// <summary>
///     Factory class for creating instances of WSDL (Web Services Description Language) readers.
///     Implements the <see cref="IWsdlFactory" /> interface to provide a mechanism for obtaining
///     WSDL reader instances.
/// </summary>
/// <remarks>
///     The <see cref="WsdlFactory" /> class is used to centralize the creation of WSDL
///     readers, such as instances of <see cref="WcfWsdlReader" />. This allows for modularity
///     and ease of extension. The factory follows a static creation pattern via
///     the <see cref="NewInstance" /> method.
/// </remarks>
public class WsdlFactory : IWsdlFactory
{
    public static IWsdlFactory NewInstance()
    {
        return new WsdlFactory();
    }

    public IWsdlReader NewWsdlReader()
    {
        return new WcfWsdlReader();
    }
}
