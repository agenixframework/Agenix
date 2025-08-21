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
///     Abstract base class for interactions that involve waiting with timeout functionality.
/// </summary>
public abstract class WaitWithTimeout : Interaction
{
    /// <summary>
    ///     Represents the maximum duration for a timeout to wait before proceeding with the interaction.
    ///     This field is used to configure and store the timeout value as a <see cref="TimeSpan" /> during
    ///     the execution of interactions that require waiting.
    /// </summary>
    protected TimeSpan Timeout;

    /// <summary>
    ///     Defines the interaction to be performed by the specified actor asynchronously.
    /// </summary>
    /// <param name="actor">The actor performing the interaction.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <typeparam name="T">The type of the actor performing the interaction.</typeparam>
    /// <returns>A task representing the asynchronous operation.</returns>
    public abstract Task PerformAsAsync<T>(T actor, CancellationToken cancellationToken = default) where T : Actor;

    /// <summary>
    ///     Specifies the maximum timeout duration using a long value.
    /// </summary>
    /// <param name="newTimeout">The timeout duration.</param>
    /// <returns>A timeout builder for specifying time units.</returns>
    public TimeoutBuilder ForNoMoreThan(long newTimeout)
    {
        return new TimeoutBuilder(newTimeout, this);
    }

    /// <summary>
    ///     Specifies the maximum timeout duration using a TimeSpan.
    /// </summary>
    /// <typeparam name="T">The concrete type that extends WaitWithTimeout.</typeparam>
    /// <param name="newTimeout">The timeout duration as a TimeSpan.</param>
    /// <returns>The current instance with the timeout configured.</returns>
    public T ForNoMoreThan<T>(TimeSpan newTimeout) where T : WaitWithTimeout
    {
        Timeout = newTimeout;
        return (T)this;
    }

    /// <summary>
    ///     Builder class for configuring timeout with specific time units.
    /// </summary>
    public class TimeoutBuilder : IWithTimeUnits
    {
        private readonly long _duration;
        private readonly WaitWithTimeout _waitWithTimeout;

        /// <summary>
        ///     Initializes a new instance of the TimeoutBuilder class.
        /// </summary>
        /// <param name="duration">The duration value.</param>
        /// <param name="waitWithTimeout">The parent WaitWithTimeout instance.</param>
        public TimeoutBuilder(long duration, WaitWithTimeout waitWithTimeout)
        {
            _waitWithTimeout = waitWithTimeout;
            _duration = duration;
        }

        /// <summary>
        ///     Configures the timeout in seconds.
        /// </summary>
        /// <returns>The configured WaitWithTimeout instance.</returns>
        public IPerformable Seconds()
        {
            _waitWithTimeout.Timeout = TimeSpan.FromSeconds(_duration);
            return _waitWithTimeout;
        }

        /// <summary>
        ///     Configures the timeout in milliseconds.
        /// </summary>
        /// <returns>The configured WaitWithTimeout instance.</returns>
        public IPerformable Milliseconds()
        {
            _waitWithTimeout.Timeout = TimeSpan.FromMilliseconds(_duration);
            return _waitWithTimeout;
        }

        /// <summary>
        ///     Configures the timeout in minutes.
        /// </summary>
        /// <returns>The configured WaitWithTimeout instance.</returns>
        public IPerformable Minutes()
        {
            _waitWithTimeout.Timeout = TimeSpan.FromMinutes(_duration);
            return _waitWithTimeout;
        }
    }
}
