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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reqnroll;

namespace Agenix.ReqnrollPlugin;

/// <summary>
///     Provides a custom equality comparer for comparing two <see cref="FeatureInfo" /> objects
///     based on their properties such as Title, Description, GenerationTargetLanguage,
///     and Tags collection.
/// </summary>
internal class FeatureInfoEqualityComparer : IEqualityComparer<FeatureInfo>
{
    public bool Equals(FeatureInfo x, FeatureInfo y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x?.Title == y?.Title
            && x?.Description == y?.Description
            && x?.GenerationTargetLanguage == y?.GenerationTargetLanguage
            && x.Tags.SequenceEqual(y.Tags))
        {
            return true;
        }

        return false;
    }

    public int GetHashCode(FeatureInfo obj)
    {
        return GetFeatureInfoHashCode(obj);
    }

    public static int GetFeatureInfoHashCode(FeatureInfo obj)
    {
        return obj.Title.GetHashCode()
               ^ (obj.Description ?? string.Empty).GetHashCode()
               ^ obj.GenerationTargetLanguage.GetHashCode()
               ^ obj.Language.DisplayName.GetHashCode()
               ^ ((IStructuralEquatable)obj.Tags).GetHashCode(EqualityComparer<string>.Default);
    }
}
