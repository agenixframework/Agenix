
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

namespace Agenix.Configuration.Sample.Approach1;

/// <summary>
/// Console application to demonstrate Approach 1: Direct Instance Example
/// </summary>
public static class Program
{
    /// <summary>
    /// Entry point for the console application to demonstrate the Direct Instance Example.
    /// Handles the sequential execution of demonstration methods, including basic usage,
    /// environment-specific usage, asynchronous usage, caching and reloading, and service usage.
    /// </summary>
    /// <param name="args">Array of command-line arguments passed to the application.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous execution of the program.</returns>
    public static async Task Main(string[] args)
    {

        // Setup logging
        try
        {
            Console.WriteLine("=== Agenix Configuration - Approach 1: Direct Instance Example ===");
            Console.WriteLine();

            // Create the example instance
            var example = new DirectInstanceExample();

            // Run all demonstrations
            Console.WriteLine("Press any key to start the demonstrations...");
            Console.ReadKey();
            Console.WriteLine();

            // 1. Basic Usage
            example.DemonstrateBasicUsage();
            Console.WriteLine();

            // Wait for user input
            Console.WriteLine("Press any key to continue to Environment Specific Usage...");
            Console.ReadKey();
            Console.WriteLine();

            // 2. Environment Specific Usage
            example.DemonstrateEnvironmentSpecificUsage();
            Console.WriteLine();

            // Wait for user input
            Console.WriteLine("Press any key to continue to Async Usage...");
            Console.ReadKey();
            Console.WriteLine();

            // 3. Async Usage
            await example.DemonstrateAsyncUsage();
            Console.WriteLine();

            // Wait for user input
            Console.WriteLine("Press any key to continue to Caching & Reloading...");
            Console.ReadKey();
            Console.WriteLine();

            // 4. Caching and Reloading
            example.DemonstrateCachingAndReloading();
            Console.WriteLine();

            // Wait for user input
            Console.WriteLine("Press any key to continue to Service Usage...");
            Console.ReadKey();
            Console.WriteLine();

            // 5. Service Usage
            example.DemonstrateServiceUsage();
            Console.WriteLine();

            Console.WriteLine("=== All demonstrations completed! ===");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
