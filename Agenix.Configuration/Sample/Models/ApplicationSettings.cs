
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

#endregion

namespace Agenix.Configuration.Sample.Models;

/// <summary>
/// Sample application settings for demonstrating configuration usage.
/// </summary>
public class ApplicationSettings
{
    /// <summary>
    /// Database connection string
    /// </summary>
    public string DatabaseConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// API base URL
    /// </summary>
    public string ApiBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Logging configuration
    /// </summary>
    public LoggingSettings Logging { get; set; } = new();

    /// <summary>
    /// Cache configuration
    /// </summary>
    public CacheSettings Cache { get; set; } = new();

    /// <summary>
    /// Feature flags
    /// </summary>
    public FeatureFlags Features { get; set; } = new();
}

/// <summary>
/// Logging configuration settings
/// </summary>
public class LoggingSettings
{
    /// <summary>
    /// Minimum log level
    /// </summary>
    public string Level { get; set; } = "Information";

    /// <summary>
    /// Log file path
    /// </summary>
    public string FilePath { get; set; } = "./logs/app.log";

    /// <summary>
    /// Enable console logging
    /// </summary>
    public bool EnableConsole { get; set; } = true;
}

/// <summary>
/// Cache configuration settings
/// </summary>
public class CacheSettings
{
    /// <summary>
    /// Cache provider type
    /// </summary>
    public string Provider { get; set; } = "Memory";

    /// <summary>
    /// Cache expiration time in minutes
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Redis connection string (if using Redis)
    /// </summary>
    public string? RedisConnectionString { get; set; }
}

/// <summary>
/// Feature flags configuration
/// </summary>
public class FeatureFlags
{
    /// <summary>
    /// Enable new dashboard feature
    /// </summary>
    public bool EnableNewDashboard { get; set; } = false;

    /// <summary>
    /// Enable advanced analytics
    /// </summary>
    public bool EnableAdvancedAnalytics { get; set; } = true;

    /// <summary>
    /// Enable beta features
    /// </summary>
    public bool EnableBetaFeatures { get; set; } = false;
}
