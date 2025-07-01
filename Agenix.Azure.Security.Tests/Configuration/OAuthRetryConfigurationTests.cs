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

using Agenix.Azure.Security.Configuration;

namespace Agenix.Azure.Security.Tests.Configuration;

[TestFixture]
public class OAuthRetryConfigurationTests
{
    private OAuthRetryConfiguration _retryConfig;

    [SetUp]
    public void SetUp()
    {
        _retryConfig = new OAuthRetryConfiguration();
    }

    [Test]
    public void Constructor_DefaultValues_AreSetCorrectly()
    {
        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(_retryConfig.MaxRetries, Is.EqualTo(3));
            Assert.That(_retryConfig.InitialDelay, Is.EqualTo(TimeSpan.FromSeconds(1)));
            Assert.That(_retryConfig.MaxDelay, Is.EqualTo(TimeSpan.FromSeconds(30)));
            Assert.That(_retryConfig.DelayMultiplier, Is.EqualTo(2.0));
            Assert.That(_retryConfig.UseJitter, Is.True);
        });
    }

    [Test]
    public void GetDelay_FirstAttempt_ReturnsInitialDelay()
    {
        // Arrange
        _retryConfig.UseJitter = false; // Disable jitter for predictable testing

        // Act
        var delay = _retryConfig.GetDelay(0);

        // Assert
        Assert.That(delay, Is.EqualTo(_retryConfig.InitialDelay));
    }

    [Test]
    public void GetDelay_SecondAttempt_ReturnsDelayWithMultiplier()
    {
        // Arrange
        _retryConfig.UseJitter = false;
        _retryConfig.InitialDelay = TimeSpan.FromSeconds(2);
        _retryConfig.DelayMultiplier = 3.0;

        // Act
        var delay = _retryConfig.GetDelay(1);

        // Assert
        Assert.That(delay, Is.EqualTo(TimeSpan.FromSeconds(6))); // 2 * 3^1
    }

    [Test]
    public void GetDelay_ThirdAttempt_ReturnsExponentialBackoff()
    {
        // Arrange
        _retryConfig.UseJitter = false;
        _retryConfig.InitialDelay = TimeSpan.FromSeconds(1);
        _retryConfig.DelayMultiplier = 2.0;

        // Act
        var delay = _retryConfig.GetDelay(2);

        // Assert
        Assert.That(delay, Is.EqualTo(TimeSpan.FromSeconds(4))); // 1 * 2^2
    }

    [Test]
    public void GetDelay_ExceedsMaxDelay_ReturnsMaxDelay()
    {
        // Arrange
        _retryConfig.UseJitter = false;
        _retryConfig.InitialDelay = TimeSpan.FromSeconds(10);
        _retryConfig.MaxDelay = TimeSpan.FromSeconds(15);
        _retryConfig.DelayMultiplier = 3.0;

        // Act
        var delay = _retryConfig.GetDelay(2); // Would be 10 * 3^2 = 90 seconds

        // Assert
        Assert.That(delay, Is.EqualTo(_retryConfig.MaxDelay));
    }

    [Test]
    public void GetDelay_WithJitter_ReturnsDelayWithVariation()
    {
        // Arrange
        _retryConfig.UseJitter = true;
        _retryConfig.InitialDelay = TimeSpan.FromSeconds(10);
        var expectedBaseDelay = TimeSpan.FromSeconds(10);

        // Act
        var delays = new List<TimeSpan>();
        for (int i = 0; i < 10; i++)
        {
            delays.Add(_retryConfig.GetDelay(0));
        }

        // Assert
        Assert.That(delays, Has.All.GreaterThanOrEqualTo(expectedBaseDelay));
        Assert.That(delays, Has.All.LessThanOrEqualTo(TimeSpan.FromSeconds(11))); // Max 10% jitter

        // Check that we get some variation (not all delays are the same)
        var uniqueDelays = delays.Distinct().Count();
        Assert.That(uniqueDelays, Is.GreaterThan(1));
    }

    [Test]
    public void GetDelay_WithoutJitter_ReturnsConsistentDelay()
    {
        // Arrange
        _retryConfig.UseJitter = false;

        // Act
        var delay1 = _retryConfig.GetDelay(1);
        var delay2 = _retryConfig.GetDelay(1);
        var delay3 = _retryConfig.GetDelay(1);

        // Assert
        Assert.That(delay1, Is.EqualTo(delay2));
        Assert.That(delay2, Is.EqualTo(delay3));
    }

    [Test]
    public void GetDelay_NegativeAttempt_HandlesGracefully()
    {
        // Arrange
        _retryConfig.UseJitter = false;

        // Act
        var delay = _retryConfig.GetDelay(-1);

        // Assert
        Assert.That(delay.TotalMilliseconds, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void GetDelay_LargeAttemptNumber_DoesNotExceedMaxDelay()
    {
        // Arrange
        _retryConfig.UseJitter = false;
        _retryConfig.MaxDelay = TimeSpan.FromSeconds(30);

        // Act
        var delay = _retryConfig.GetDelay(100); // Very large attempt number

        // Assert
        Assert.That(delay, Is.LessThanOrEqualTo(_retryConfig.MaxDelay));
    }

    [Test]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange & Act
        _retryConfig.MaxRetries = 5;
        _retryConfig.InitialDelay = TimeSpan.FromSeconds(2);
        _retryConfig.MaxDelay = TimeSpan.FromMinutes(1);
        _retryConfig.DelayMultiplier = 1.5;
        _retryConfig.UseJitter = false;

        // Assert
        Assert.That(_retryConfig.MaxRetries, Is.EqualTo(5));
        Assert.That(_retryConfig.InitialDelay, Is.EqualTo(TimeSpan.FromSeconds(2)));
        Assert.That(_retryConfig.MaxDelay, Is.EqualTo(TimeSpan.FromMinutes(1)));
        Assert.That(_retryConfig.DelayMultiplier, Is.EqualTo(1.5));
        Assert.That(_retryConfig.UseJitter, Is.False);
    }

    [Test]
    public void GetDelay_CustomDelayMultiplier_CalculatesCorrectly()
    {
        // Arrange
        _retryConfig.UseJitter = false;
        _retryConfig.InitialDelay = TimeSpan.FromSeconds(5);
        _retryConfig.DelayMultiplier = 1.5;

        // Act
        var delay0 = _retryConfig.GetDelay(0);
        var delay1 = _retryConfig.GetDelay(1);
        var delay2 = _retryConfig.GetDelay(2);

        // Assert
        Assert.That(delay0, Is.EqualTo(TimeSpan.FromSeconds(5))); // 5 * 1.5^0 = 5
        Assert.That(delay1, Is.EqualTo(TimeSpan.FromSeconds(7.5))); // 5 * 1.5^1 = 7.5
        Assert.That(delay2, Is.EqualTo(TimeSpan.FromSeconds(11.25))); // 5 * 1.5^2 = 11.25
    }

    [Test]
    public void GetDelay_ZeroInitialDelay_HandlesCorrectly()
    {
        // Arrange
        _retryConfig.UseJitter = false;
        _retryConfig.InitialDelay = TimeSpan.Zero;

        // Act
        var delay = _retryConfig.GetDelay(3);

        // Assert
        Assert.That(delay, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void GetDelay_JitterVariation_StaysWithinExpectedRange()
    {
        // Arrange
        _retryConfig.UseJitter = true;
        _retryConfig.InitialDelay = TimeSpan.FromSeconds(10);
        var baseDelayMs = 10000; // 10 seconds

        // Act - Run multiple times to test jitter range
        var delays = new List<double>();
        for (int i = 0; i < 100; i++)
        {
            var delay = _retryConfig.GetDelay(0);
            delays.Add(delay.TotalMilliseconds);
        }

        // Assert
        var minExpected = baseDelayMs; // Base delay
        var maxExpected = baseDelayMs * 1.1; // Base + 10% jitter

        Assert.That(delays, Has.All.GreaterThanOrEqualTo(minExpected));
        Assert.That(delays, Has.All.LessThanOrEqualTo(maxExpected));

        // Verify we actually get variation
        var minDelay = delays.Min();
        var maxDelay = delays.Max();
        Assert.That(maxDelay - minDelay, Is.GreaterThan(0));
    }
}
