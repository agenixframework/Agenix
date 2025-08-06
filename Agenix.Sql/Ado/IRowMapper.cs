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

using System.Data;

namespace Agenix.Sql.Ado;

/// <summary>
///     Represents a contract for mapping a row of data, retrieved from an <see cref="IDataReader" />,
///     into an object representation. Typically used for transforming database records into strongly-typed objects.
/// </summary>
public interface IRowMapper
{
    /// <summary>
    ///     Maps a row from a data reader to an object representation.
    /// </summary>
    /// <param name="dataReader">The <see cref="IDataReader" /> instance used to retrieve the data.</param>
    /// <param name="rowNum">The zero-based index of the row being processed within the result set.</param>
    /// <returns>An object representation of the mapped row.</returns>
    object MapRow(IDataReader dataReader, int rowNum);
}
