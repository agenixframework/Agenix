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

using System.Configuration.Internal;

namespace Agenix.Api.Util;

/// <summary>
///     Holds text position information for e.g. error reporting purposes.
/// </summary>
/// <seealso cref="ConfigXmlElement" />
/// <seealso cref="ConfigXmlAttribute" />
public interface ITextPosition : IConfigErrorInfo
{
    /// <summary>
    ///     Gets a string specifying the file/resource name related to the configuration details.
    /// </summary>
    new string Filename { get; }

    /// <summary>
    ///     Gets an integer specifying the line number related to the configuration details.
    /// </summary>
    new int LineNumber { get; }

    /// <summary>
    ///     Gets an integer specifying the line position related to the configuration details.
    /// </summary>
    int LinePosition { get; }
}
