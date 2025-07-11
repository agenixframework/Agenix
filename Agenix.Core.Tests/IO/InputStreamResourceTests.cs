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

using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.IO;

#endregion

namespace Agenix.Core.Tests.IO;

/// <summary>
///     Unit tests for the InputStreamResource class.
/// </summary>
[TestFixture]
public sealed class InputStreamResourceTests
{
    [Test]
    public void Instantiation()
    {
        FileInfo file = null;
        Stream stream = null;
        try
        {
            file = new FileInfo("Instantiation");
            stream = file.Create();
            var res = new InputStreamResource(stream, "A temporary resource.");
            ClassicAssert.IsTrue(res.IsOpen);
            ClassicAssert.IsTrue(res.Exists);
            ClassicAssert.IsNotNull(res.InputStream);
        }
        finally
        {
            try
            {
                if (stream != null)
                {
                    stream.Close();
                }

                if (file != null
                    && file.Exists)
                {
                    file.Delete();
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    [Test]
    public void InstantiationWithNull()
    {
        Assert.Throws<ArgumentNullException>(() => new InputStreamResource(null, "A null resource."));
    }

    [Test]
    public void ReadStreamMultipleTimes()
    {
        FileInfo file = null;
        Stream stream = null;
        try
        {
            file = new FileInfo("ReadStreamMultipleTimes");
            stream = file.Create();
            // attempting to read this stream twice is an error...
            var res = new InputStreamResource(stream, "A temporary resource.");
            var streamOne = res.InputStream;
            Stream streamTwo;
            Assert.Throws<InvalidOperationException>(() => streamTwo = res.InputStream); // should bail here
        }
        finally
        {
            try
            {
                if (stream != null)
                {
                    stream.Close();
                }

                if (file != null
                    && file.Exists)
                {
                    file.Delete();
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
