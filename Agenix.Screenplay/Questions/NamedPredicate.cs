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

namespace Agenix.Screenplay.Questions;

/// <summary>
///     A predicate with a name that can be used for better readability and debugging.
/// </summary>
/// <typeparam name="T">The type of the input to the predicate</typeparam>
public class NamedPredicate<T>(string name, Predicate<T> predicate)
{
    public string Name => name;

    public Predicate<T> InnerPredicate => predicate;

    public override string ToString()
    {
        return name;
    }

    public bool Invoke(T obj)
    {
        return predicate(obj);
    }

    public NamedPredicate<T> And(Predicate<T> other)
    {
        return new NamedPredicate<T>(name, x => predicate(x) && other(x));
    }

    public NamedPredicate<T> Not()
    {
        return new NamedPredicate<T>(name, x => !predicate(x));
    }

    public NamedPredicate<T> Or(Predicate<T> other)
    {
        return new NamedPredicate<T>(name, x => predicate(x) || other(x));
    }
}
