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

namespace Agenix.Screenplay.Questions.Waits;

/// <summary>
///     A wait action that waits for a supplier function to return true within a specified timeout.
/// </summary>
public class WaitOnCondition : WaitWithTimeout
{
    private readonly Func<bool> _expectedState;

    /// <summary>
    ///     Initializes a new instance of the WaitOnSupplier class.
    /// </summary>
    /// <param name="expectedState">A function that returns true when the expected state is reached.</param>
    public WaitOnCondition(Func<bool> expectedState)
    {
        _expectedState = expectedState;
        Timeout = TimeSpan.FromMilliseconds(3000);
    }

    /// <summary>
    ///     Performs the wait operation for the specified actor.
    /// </summary>
    /// <typeparam name="T">The type of actor performing this action.</typeparam>
    /// <param name="actor">The actor performing this wait action.</param>
    public override void PerformAs<T>(T actor)
    {
        var endTime = DateTime.UtcNow.Add(Timeout);
        var pollInterval = TimeSpan.FromMilliseconds(100); // Default polling interval

        while (DateTime.UtcNow < endTime)
        {
            if (_expectedState())
            {
                return; // Condition met
            }

            Thread.Sleep(pollInterval);
        }

        // If we get here, the timeout was exceeded
        throw new TimeoutException($"Expected state was not reached within {Timeout.TotalMilliseconds}ms");
    }
}
