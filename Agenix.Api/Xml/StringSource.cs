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

using System.Text;

namespace Agenix.Api.Xml;

/// <summary>
///     A simple stream source representation of a static String content. Can be read many times and uses default encoding
///     set via configuration settings.
/// </summary>
public class StringSource
{
    /// <summary>
    ///     Constructor using source content as String.
    /// </summary>
    /// <param name="content">the content</param>
    public StringSource(string content)
        : this(content, AgenixSettings.AgenixFileEncoding())
    {
    }

    /// <summary>
    ///     Constructor using source content as String and encoding.
    /// </summary>
    /// <param name="content">the content</param>
    /// <param name="encoding">the encoding</param>
    public StringSource(string content, Encoding encoding)
    {
        Content = content;
        Encoding = encoding;
    }

    /// <summary>
    ///     Constructor using source content as String and encoding name.
    /// </summary>
    /// <param name="content">the content</param>
    /// <param name="encodingName">the encoding name</param>
    public StringSource(string content, string encodingName)
    {
        Content = content;
        Encoding = Encoding.GetEncoding(encodingName);
    }

    /// <summary>
    ///     Obtains the content.
    /// </summary>
    /// <returns>the content</returns>
    public string Content { get; }

    /// <summary>
    ///     Obtains the encoding.
    /// </summary>
    /// <returns>the encoding</returns>
    public Encoding Encoding { get; }

    /// <summary>
    ///     Obtains the encoding name.
    /// </summary>
    /// <returns>the encoding name</returns>
    public string EncodingName => Encoding.WebName;

    public TextReader GetReader()
    {
        return new StringReader(Content);
    }

    public Stream GetInputStream()
    {
        return new MemoryStream(Encoding.GetBytes(Content));
    }

    public override string ToString()
    {
        return Content;
    }
}
