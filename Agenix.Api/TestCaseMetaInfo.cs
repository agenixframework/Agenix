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

namespace Agenix.Core;

/// Test case meta-information.
/// /
public class TestCaseMetaInfo
{
    /// Represents the status of a test case.
    public enum Status
    {
        DRAFT,
        READY_FOR_REVIEW,
        FINAL,
        DISABLED
    }

    /// Stores the initial creation date of the test case.
    /// /
    private DateTime _creationDate;

    /// Stores the last updated date and time of the test case.
    private DateTime _lastUpdatedOn;

    public Status _status = Status.DRAFT;

    /// Represents the individual or entity responsible for the creation or ownership
    /// of the test case.
    public string Author { get; set; }

    /// Stores information about the user who last updated the test case.
    public string LastUpdatedBy { get; set; }

    /// Represents the creation date of the test case.
    /// This indicates when the test case was initially defined or created.
    public DateTime CreationDate
    {
        get => new(_creationDate.Ticks);
        set => _creationDate = new DateTime(value.Ticks);
    }

    /// Stores the last updated date and time of the test case.
    public DateTime LastUpdatedOn
    {
        get => new(_lastUpdatedOn.Ticks);
        set => _lastUpdatedOn = new DateTime(value.Ticks);
    }

    /// Sets the status of the test case.
    /// <param name="newStatus">The new status to be assigned to the test case.</param>
    /// /
    public void SetStatus(Status newStatus)
    {
        _status = newStatus;
    }

    /// Retrieves the current status of the test case.
    /// <return>The current status assigned to the test case.</return>
    public Status GetStatus()
    {
        return _status;
    }
}
