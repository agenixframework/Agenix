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

using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.IO;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Core.Endpoint;
using Agenix.Core.Util;
using Agenix.Selenium.Actions;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Safari;
using OpenQA.Selenium.Support.Events;

namespace Agenix.Selenium.Endpoint;

/// <summary>
///     Selenium browser provides access to web driver and initializes Selenium environment from endpoint configuration.
/// </summary>
public class SeleniumBrowser : AbstractEndpoint, IProducer, IDisposable
{
    /// <summary>
    ///     Represents the logger instance used for logging messages and events within the SeleniumBrowser class.
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(SeleniumBrowser));

    /// <summary>
    ///     Holds the configuration settings for initializing the Selenium browser environment,
    ///     including browser type, remote server URL, event handlers, and other related properties.
    /// </summary>
    private readonly SeleniumBrowserConfiguration _endpointConfiguration;

    /// <summary>
    ///     Represents the directory path used for temporary storage of files and data within the SeleniumBrowser class.
    /// </summary>
    private readonly string _temporaryStorage;


    /// <summary>
    ///     Provides functionality for managing Selenium WebDriver instances
    ///     with configurations and operations for browser automation.
    ///     This class serves as an endpoint for sending and receiving messages
    ///     as part of the messaging producer-consumer pattern.
    ///     Inherits from <see cref="AbstractEndpoint" /> and implements <see cref="IProducer" /> and
    ///     <see cref="IDisposable" />.
    /// </summary>
    public SeleniumBrowser() : this(new SeleniumBrowserConfiguration()) { }

    /// <summary>
    ///     Provides functionality for handling Selenium WebDriver instances
    ///     with configurations, file storage capabilities, and operations
    ///     for browser automation. Implements messaging producer-consumer patterns
    ///     and resource management with <see cref="IDisposable" />.
    ///     Inherits from <see cref="AbstractEndpoint" /> and implements <see cref="IProducer" />.
    /// </summary>
    public SeleniumBrowser(SeleniumBrowserConfiguration endpointConfiguration) : base(endpointConfiguration)
    {
        _endpointConfiguration = endpointConfiguration;
        _temporaryStorage = CreateTemporaryStorage();
    }

    /// <summary>
    ///     Gets the endpoint configuration
    /// </summary>
    public override SeleniumBrowserConfiguration EndpointConfiguration => _endpointConfiguration;

    /// <summary>
    ///     Gets the web driver
    /// </summary>
    /// <summary>
    ///     Gets or sets the web driver
    /// </summary>
    public virtual IWebDriver? WebDriver { get; set; }

    /// <summary>
    ///     Gets whether the browser is started
    /// </summary>
    public virtual bool IsStarted => WebDriver != null;

    private bool _disposed;

    /// <summary>
    ///     Disposes of the browser resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the resources used by the SeleniumBrowser instance.
    /// This method is called to clean up both managed and unmanaged resources
    /// held by the SeleniumBrowser class. Implements the <see cref="IDisposable"/> pattern.
    /// </summary>
    /// <param name="disposing">
    /// Indicates whether the method is being called explicitly (true)
    /// or by the runtime garbage collector (false). When true, managed
    /// resources as well as unmanaged resources are disposed; otherwise, only
    /// unmanaged resources are released.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose of managed resources
                Stop();
            }

            // Dispose unmanaged resources
            CleanupTemporaryStorage();

            _disposed = true;
        }
    }

    private void CleanupTemporaryStorage()
    {
        try
        {
            if (Directory.Exists(_temporaryStorage))
            {
                Directory.Delete(_temporaryStorage, true);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to clean up temporary storage: {e.Message}");
        }
    }

    /// <summary>
    ///     Sends a message using the specified messaging infrastructure and test context,
    ///     enabling the execution of Selenium actions defined within the message payload.
    /// </summary>
    /// <param name="message">
    ///     Represents the message to be sent. The message contains a payload of type
    ///     <see cref="ISeleniumAction" /> which will be executed within the provided context.
    /// </param>
    /// <param name="context">
    ///     The test context in which the action specified in the message payload will be executed.
    /// </param>
    public void Send(IMessage message, TestContext context)
    {
        var action = message.GetPayload<ISeleniumAction>();
        action.Execute(context);

        Logger.LogInformation("Selenium action successfully executed");
    }

    /// <summary>
    ///     Starts the browser and creates local or remote web driver
    /// </summary>
    public virtual void Start()
    {
        if (!IsStarted)
        {
            if (_endpointConfiguration.WebDriver != null)
            {
                WebDriver = _endpointConfiguration.WebDriver;
            }
            else if (!string.IsNullOrEmpty(_endpointConfiguration.RemoteServerUrl))
            {
                WebDriver = CreateRemoteWebDriver(_endpointConfiguration.BrowserType,
                    _endpointConfiguration.RemoteServerUrl);
            }
            else
            {
                WebDriver = CreateLocalWebDriver(_endpointConfiguration.BrowserType);
            }

            if (_endpointConfiguration.EventHandlers != null && _endpointConfiguration.EventHandlers.Count != 0)
            {
                Logger.LogInformation("Add event listeners to web driver {EventHandlerCount}",
                    _endpointConfiguration.EventHandlers.Count);
                var eventFiringDriver = new EventFiringWebDriver(WebDriver);

                // Apply all the event handlers from configuration
                foreach (var eventHandler in _endpointConfiguration.EventHandlers)
                {
                    eventHandler(eventFiringDriver);
                }

                // Replace your original driver with the event-firing one
                WebDriver = eventFiringDriver;
            }
        }
        else
        {
            Logger.LogDebug("Browser already started");
        }
    }

    /// <summary>
    ///     Stops the browser when started
    /// </summary>
    public virtual void Stop()
    {
        if (IsStarted)
        {
            Logger.LogInformation("Stopping browser {WebDriverUrl}", WebDriver.Url);

            try
            {
                Logger.LogInformation("Trying to close the browser {WebDriver1}...", WebDriver);
                WebDriver.Quit();
            }
            catch (WebDriverException e)
            {
                Logger.LogError(e, "Browser is unreachable or failed to close");
            }

            WebDriver = null;
        }
        else
        {
            Logger.LogWarning("Browser already stopped");
        }
    }

    /// <summary>
    /// Stores a file in the browser's temporary storage, making it available for further operations.
    /// </summary>
    /// <param name="filePath">
    /// The file path to be stored. This path specifies the location of the file to be processed.
    /// </param>
    /// <returns>
    /// The storage path where the file has been successfully stored.
    /// </returns>
    public string StoreFile(string filePath)
    {
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
        var testDir = Path.GetDirectoryName(testFilePath);

        if (!Directory.Exists(testDir))
        {
            Directory.CreateDirectory(testDir);
        }

        // Create the file if it doesn't exist
        if (!File.Exists(testFilePath))
        {
            File.WriteAllText(testFilePath, string.Empty); // Create an empty file
        }


        return StoreFile(FileUtils.GetFileResource(filePath));
    }

    /// <summary>
    ///     Stores a file in temporary storage
    /// </summary>
    /// <param name="filePath">Path to the file to store</param>
    /// <returns>Path to the stored file</returns>
    /// <summary>
    ///     Stores a file from an IResource to temporary storage
    /// </summary>
    /// <param name="resource">The resource containing the file to store</param>
    /// <returns>Canonical path to the stored file</returns>
    public string StoreFile(IResource resource)
    {
        try
        {
            if (resource == null)
            {
                throw new AgenixSystemException("Resource cannot be null");
            }

            // Ensure a temporary storage directory exists
            if (!Directory.Exists(_temporaryStorage))
            {
                Directory.CreateDirectory(_temporaryStorage);
            }

            // Get filename from resource URI or use a default name
            var fileName = FileUtils.GetFileName(resource.File.FullName);
            var newFilePath = Path.Combine(_temporaryStorage, fileName);

            Logger.LogInformation("Store file from resource {ResourceUri} to {NewFilePath}",
                resource.Uri?.ToString() ?? "unknown", newFilePath);

            // Copy the resource content to the new file

            using (var inputStream = resource.InputStream)
            using (var outputStream = new FileStream(newFilePath, FileMode.Create, FileAccess.Write))
            {
                inputStream.CopyTo(outputStream);
            }

            return Path.GetFullPath(newFilePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new AgenixSystemException($"Access denied when storing resource file. Check directory permissions for: {_temporaryStorage}", ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new AgenixSystemException($"Directory not found when storing resource file: {_temporaryStorage}", ex);
        }
        catch (IOException ex)
        {
            throw new AgenixSystemException($"I/O error when storing resource file. Error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new AgenixSystemException($"Failed to store file from resource: {resource?.Uri?.ToString() ?? "unknown"}", ex);
        }
    }


    /// <summary>
    ///     Retrieves a stored file path
    /// </summary>
    /// <param name="filename">Name of the file to retrieve</param>
    /// <returns>Full path to the stored file</returns>
    public virtual string GetStoredFile(string filename)
    {
        try
        {
            var storedPath = Path.Combine(_temporaryStorage, filename);

            if (!File.Exists(storedPath))
            {
                throw new FileNotFoundException($"Failed to access stored file: {storedPath}");
            }

            return Path.GetFullPath(storedPath);
        }
        catch (Exception e)
        {
            throw new AgenixSystemException($"Failed to retrieve file: {filename}", e);
        }
    }

    /// <summary>
    ///     Creates a local web driver
    /// </summary>
    /// <param name="browserType">Type of browser to create</param>
    /// <returns>WebDriver instance</returns>
    private IWebDriver CreateLocalWebDriver(string browserType)
    {
        switch (browserType.ToLower())
        {
            case "firefox":
                var firefoxOptions = new FirefoxOptions { Profile = _endpointConfiguration.FirefoxProfile };
                // Set custom download folder
                firefoxOptions.Profile.SetPreference("browser.download.dir", _temporaryStorage);
                return new FirefoxDriver(firefoxOptions);

            case "chrome":
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddUserProfilePreference("download.default_directory", _temporaryStorage);
                return new ChromeDriver(chromeOptions);

            case "edge":
                var edgeOptions = new EdgeOptions();
                return new EdgeDriver(edgeOptions);

            case "safari":
                return new SafariDriver();

            case "internetexplorer":
            case "ie":
                return new InternetExplorerDriver();

            case "htmlunit":
                // Note: HtmlUnit is not directly available in C# Selenium
                // You might need to use a different approach or a third-party library
                throw new NotSupportedException(
                    "HtmlUnit driver is not available in C# Selenium. Consider using Chrome in headless mode.");

            default:
                throw new AgenixSystemException($"Unsupported local browser type: {browserType}");
        }
    }

    /// <summary>
    ///     Creates a remote web driver
    /// </summary>
    /// <param name="browserType">Type of browser</param>
    /// <param name="serverAddress">Remote server address</param>
    /// <returns>RemoteWebDriver instance</returns>
    private RemoteWebDriver CreateRemoteWebDriver(string browserType, string serverAddress)
    {
        try
        {
            DriverOptions? options = null;

            switch (browserType.ToLower())
            {
                case "firefox":
                    var firefoxOptions = new FirefoxOptions();
                    firefoxOptions.Profile = _endpointConfiguration.FirefoxProfile;
                    options = firefoxOptions;
                    break;

                case "chrome":
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("--ignore-certificate-errors");
                    options = chromeOptions;
                    break;

                case "edge":
                    var edgeOptions = new EdgeOptions();
                    edgeOptions.AddArgument("--ignore-certificate-errors");
                    options = edgeOptions;
                    break;

                case "internetexplorer":
                case "ie":
                    var ieOptions = new InternetExplorerOptions();
                    ieOptions.AddAdditionalInternetExplorerOption("acceptInsecureCerts", true);
                    break;

                default:
                    throw new AgenixSystemException($"Unsupported remote browser type: {browserType}");
            }

            return new RemoteWebDriver(new Uri(serverAddress), options);
        }
        catch (UriFormatException e)
        {
            throw new AgenixSystemException("Failed to access remote server", e);
        }
    }

    /// <summary>
    ///     Creates a temporary storage directory
    /// </summary>
    /// <returns>Path to temporary directory</returns>
    private static string CreateTemporaryStorage()
    {
        try
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "selenium_" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(tempDir);

            Logger.LogInformation("Download storage location is: {TempDir}", tempDir);
            return tempDir;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Could not create temporary storage", e);
        }
    }


    /// <summary>
    ///     Creates and returns an instance of <see cref="IProducer" /> for managing
    ///     messaging production. This method overrides the base implementation
    ///     to provide a concrete producer instance tied to the current object.
    /// </summary>
    /// <returns>An instance of <see cref="IProducer" /> representing the producer for messaging operations.</returns>
    public override IProducer CreateProducer()
    {
        return this;
    }

    /// <summary>
    ///     Creates a message consumer for processing inbound messages.
    ///     This method is overridden in derived classes to provide specific consumer implementation.
    /// </summary>
    /// <returns>
    ///     An instance of <see cref="IConsumer" />, representing the message consumer.
    ///     Throws <see cref="UnsupportedOperationException" /> if the operation is not supported.
    /// </returns>
    public override IConsumer CreateConsumer()
    {
        throw new UnsupportedOperationException("Selenium browser must not be used as message consumer");
    }
}
