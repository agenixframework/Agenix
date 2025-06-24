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

using Agenix.Validation.Json.Json;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.Json.Tests.Json;

public class JsonPathUtilsTest : AbstractNUnitSetUp
{
    private readonly string _jsonSource = @"{
                              'Stores': [
                                'Lambton Quay',
                                'Willis Street'
                              ],
                              'Manufacturers': [
                                {
                                  'Name': 'Acme Co',
                                  'Products': [
                                    {
                                      'Name': 'Anvil',
                                      'Price': 50
                                    }
                                  ]
                                },
                                {
                                  'Name': 'Contoso',
                                  'Products': [
                                    {
                                      'Name': 'Elbow Grease',
                                      'Price': 99.95
                                    },
                                    {
                                      'Name': 'Headlight Fluid',
                                      'Price': 4
                                    }
                                  ]
                                }
                              ]
                            }";

    [Test]
    public void TestEvaluateAsString()
    {
        const string keySetOfManufacturersExpression = "$.Manufacturers[?(@.Name == 'Acme Co')].KeySet()";
        Assert.That(JsonPathUtils.EvaluateAsString(_jsonSource, keySetOfManufacturersExpression),
            Is.EqualTo("Name, Products"));

        const string oneSpecificManufacturerExpression = "$.Manufacturers[?(@.Name == 'Acme Co')]";
        Assert.That(
            JsonPathUtils.EvaluateAsString(_jsonSource, oneSpecificManufacturerExpression),
            Is.EqualTo("{\"Name\":\"Acme Co\",\"Products\":[{\"Name\":\"Anvil\",\"Price\":50}]}"));

        const string manufacturersSize = "$.Manufacturers.Size()";
        Assert.That(JsonPathUtils.EvaluateAsString(_jsonSource, manufacturersSize),
            Is.EqualTo("2"));

        const string listOfProductNamesWherePriceGreaterThanThree = "$..Products[?(@.Price > 3)].Name";
        Assert.That(JsonPathUtils.EvaluateAsString(_jsonSource, listOfProductNamesWherePriceGreaterThanThree),
            Is.EqualTo("Anvil, Elbow Grease, Headlight Fluid"));
    }
}
