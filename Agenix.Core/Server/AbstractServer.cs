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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Agenix.Api.Common;
using Agenix.Api.Endpoint;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Api.Server;
using Agenix.Api.Spi;
using Agenix.Core.Endpoint;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Message;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Server;

/// <summary>
/// Represents an abstract server providing the base implementation for server functionality.
/// </summary>
/// <remarks>
/// This abstract class extends <see cref="AbstractEndpoint"/> and implements multiple server-related interfaces, including <see cref="IServer"/>, <see cref="InitializingPhase"/>, <see cref="IShutdownPhase"/>, and <see cref="IReferenceResolverAware"/>.
/// It serves as the foundation for defining server behavior, lifecycle management, and configuration handling.
/// </remarks>
public abstract class AbstractServer : AbstractEndpoint, IServer, InitializingPhase, IShutdownPhase,
    IReferenceResolverAware
{
    /// <summary>
    /// Default in memory queue suffix
    /// </summary>
    public const string DefaultChannelIdSuffix = ".inbound";

    /// <summary>
    /// Running flag
    /// </summary>
    private volatile bool _running;

    /// <summary>
    /// Autostart server after properties are set
    /// </summary>
    private bool _autoStart;

    /// <summary>
    /// Thread running the server
    /// </summary>
    private Thread _thread;

    /// <summary>
    /// Monitor for startup and running lifecycle
    /// </summary>
    private readonly object _runningLock = new();

    /// <summary>
    /// Reference resolver injected
    /// </summary>
    private IReferenceResolver _referenceResolver;

    /// <summary>
    /// Message endpoint adapter for incoming requests
    /// </summary>
    private IEndpointAdapter _endpointAdapter;

    /// <summary>
    /// Handler interceptors such as security or logging interceptors
    /// </summary>
    private List<object> _interceptors = [];

    /// <summary>
    /// Timeout delegated to default endpoint adapter if not set explicitly
    /// </summary>
    private long _defaultTimeout = 1000;

    /// <summary>
    /// Inbound memory queue debug logging
    /// </summary>
    private bool _debugLogging;

    /// <summary>
    /// Logger
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// Default constructor using endpoint configuration
    /// </summary>
    protected AbstractServer() : base(null)
    {
        Logger = LogManager.GetLogger(typeof(AbstractServer));
    }

    /// <summary>
    /// Constructor with logger
    /// </summary>
    protected AbstractServer(ILogger logger) : base(null)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Start the server - C# equivalent of Java's start() method
    /// </summary>
    public virtual void Start()
    {
        Logger.LogDebug("Starting server: {ServerName} ...", Name);

        Startup();

        lock (_runningLock)
        {
            _running = true;
        }

        // Create and start thread similar to Java's threading model
        _thread = new Thread(new ThreadStart(Run))
        {
            IsBackground = false, // Equivalent to setDaemon(false) in Java
            Name = $"{Name}-ServerThread"
        };
        _thread.Start();

        Logger.LogInformation("Started server: {ServerName}", Name);
    }

    /// <summary>
    /// Stop the server - C# equivalent of Java's stop() method
    /// </summary>
    public virtual void Stop()
    {
        if (IsRunning())
        {
            Logger.LogDebug("Stopping server: {ServerName} ...", Name);

            Shutdown();

            lock (_runningLock)
            {
                _running = false;
            }

            _thread = null;

            Logger.LogInformation("Stopped server: {ServerName}", Name);
        }
    }

    /// <summary>
    /// Implementation of IRunnable.Run() - C# equivalent of Java's Runnable.run()
    /// Subclasses may overwrite this method in order to add special execution logic.
    /// </summary>
    public virtual void Run()
    {
        // Default implementation - keep thread alive while running
        // This mimics the Java behavior where the thread stays alive
        while (IsRunning())
        {
            try
            {
                Thread.Sleep(100); // Small sleep to prevent busy waiting
            }
            catch (ThreadInterruptedException)
            {
                // Thread was interrupted, exit the loop
                break;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in server run loop");
                break;
            }
        }
    }

    /// <summary>
    /// Subclasses must implement this method called on server startup
    /// </summary>
    protected abstract void Startup();

    /// <summary>
    /// Subclasses must implement this method called on server shutdown
    /// </summary>
    protected abstract void Shutdown();

    /// <summary>
    /// Initialize the server
    /// </summary>
    public virtual void Initialize()
    {
        if (_endpointAdapter == null)
        {
            // The server inbound queue
            IMessageQueue inboundQueue;
            var queueName = Name + DefaultChannelIdSuffix;

            if (_referenceResolver != null && _referenceResolver.IsResolvable(queueName))
            {
                inboundQueue = _referenceResolver.Resolve<IMessageQueue>(queueName);
            }
            else
            {
                inboundQueue = new DefaultMessageQueue(queueName);
            }

            if (inboundQueue is DefaultMessageQueue defaultQueue)
            {
                defaultQueue.SetLoggingEnabled(_debugLogging);
            }

            var directEndpointConfiguration = new DirectSyncEndpointConfiguration { Timeout = _defaultTimeout };

            directEndpointConfiguration.SetQueue(inboundQueue);

            _endpointAdapter = new DirectEndpointAdapter(directEndpointConfiguration);
            _endpointAdapter.GetEndpoint().SetName(Name);

            if (_endpointAdapter is DirectEndpointAdapter directAdapter)
            {
                directAdapter.TestContextFactory = GetTestContextFactory();
            }
        }

        if (_autoStart && !IsRunning())
        {
            Start();
        }
    }

    /// <summary>
    /// Get test context factory
    /// </summary>
    private TestContextFactory GetTestContextFactory()
    {
        if (_referenceResolver != null)
        {
            var factories = _referenceResolver.ResolveAll<TestContextFactory>();
            if (factories.Any())
            {
                return factories.First().Value;
            }
        }

        Logger.LogDebug(
            "Unable to create test context factory from reference resolver - using minimal test context factory");
        return TestContextFactory.NewInstance();
    }

    /// <summary>
    /// Destroy the server
    /// </summary>
    public virtual void Destroy()
    {
        if (IsRunning())
        {
            Stop();
        }
    }

    /// <summary>
    /// Join server thread - C# equivalent of Java's thread.join()
    /// </summary>
    public void Join()
    {
        try
        {
            _thread?.Join();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred while joining server thread");
        }
    }

    /// <summary>
    /// Check if server is running
    /// </summary>
    public bool IsRunning()
    {
        lock (_runningLock)
        {
            return _running;
        }
    }

    /// <summary>
    /// Get endpoint configuration
    /// </summary>
    public override IEndpointConfiguration EndpointConfiguration =>
        _endpointAdapter?.GetEndpoint()?.EndpointConfiguration;

    /// <summary>
    /// Create consumer
    /// </summary>
    public override IConsumer CreateConsumer()
    {
        return _endpointAdapter?.GetEndpoint()?.CreateConsumer();
    }

    /// <summary>
    /// Create producer
    /// </summary>
    public override IProducer CreateProducer()
    {
        return _endpointAdapter?.GetEndpoint()?.CreateProducer();
    }

    #region Properties - C# equivalent of Java getters/setters

    /// <summary>
    /// Enable/disable server auto start
    /// </summary>
    public bool AutoStart
    {
        get => _autoStart;
        set => _autoStart = value;
    }

    /// <summary>
    /// Gets or sets the running state
    /// </summary>
    public bool Running
    {
        get => _running;
        set => _running = value;
    }

    /// <summary>
    /// Gets the reference resolver
    /// </summary>
    public IReferenceResolver ReferenceResolver
    {
        get => _referenceResolver;
        set => _referenceResolver = value;
    }

    /// <summary>
    /// Gets or sets the message endpoint adapter
    /// </summary>
    public IEndpointAdapter EndpointAdapter
    {
        get => _endpointAdapter;
        set => _endpointAdapter = value;
    }

    /// <summary>
    /// Gets or sets the handler interceptors
    /// </summary>
    public List<object> Interceptors
    {
        get => _interceptors;
        set => _interceptors = value ?? [];
    }

    /// <summary>
    /// Gets or sets the default timeout for sending and receiving messages
    /// </summary>
    public long DefaultTimeout
    {
        get => _defaultTimeout;
        set => _defaultTimeout = value;
    }

    /// <summary>
    /// Gets or sets debug logging
    /// </summary>
    public bool DebugLogging
    {
        get => _debugLogging;
        set => _debugLogging = value;
    }

    #endregion

    public void SetReferenceResolver(IReferenceResolver referenceResolver)
    {
        throw new NotImplementedException();
    }
}
