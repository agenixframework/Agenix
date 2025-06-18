#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Globalization;
using System.Threading;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Core.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Actions;

/// <summary>
///     Stop the test execution for a given amount of time.
/// </summary>
/// <param name="builder"></param>
public class SleepAction(SleepAction.Builder builder) : AbstractTestAction("sleep", builder)
{
    /// Logger for SleepAction.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(SleepAction));

    public string Time { get; } = builder.time;

    public ScheduledExecutor.TimeUnit TimeUnit { get; } = builder.timeUnit;

    /// <summary>
    ///     Executes the sleep action, causing the current thread to pause execution for the specified duration.
    /// </summary>
    /// <param name="context">The test context that provides the dynamic resolution of the sleep duration value.</param>
    public override void DoExecute(TestContext context)
    {
        var duration = context.ResolveDynamicValue(Time);

        try
        {
            TimeSpan parsedDuration;
            if (duration.IndexOf('.', StringComparison.Ordinal) > 0)
            {
                parsedDuration = TimeUnit switch
                {
                    ScheduledExecutor.TimeUnit.MILLISECONDS => TimeSpan.FromMilliseconds(Math.Round(double.Parse(
                        duration, CultureInfo.InvariantCulture
                    ))),
                    ScheduledExecutor.TimeUnit.SECONDS => TimeSpan.FromSeconds(
                        Math.Round(double.Parse(duration, CultureInfo.InvariantCulture))),
                    ScheduledExecutor.TimeUnit.MINUTES => TimeSpan.FromMinutes(
                        Math.Round(double.Parse(duration, CultureInfo.InvariantCulture))),
                    _ => throw new AgenixSystemException(
                        "Unsupported time expression for sleep action - please use one of milliseconds, seconds, minutes")
                };
            }
            else
            {
                parsedDuration = TimeSpan.FromMilliseconds(
                    ConvertToMilliseconds(long.Parse(duration), TimeUnit));
            }

            Log.LogInformation($"Sleeping {duration} {TimeUnit}");

            Thread.Sleep(parsedDuration);

            Log.LogInformation($"Returning after {duration} {TimeUnit}");
        }
        catch (ThreadInterruptedException e)
        {
            throw new AgenixSystemException(e.Message);
        }
    }

    private static long ConvertToMilliseconds(long value, ScheduledExecutor.TimeUnit sourceUnit)
    {
        return sourceUnit switch
        {
            ScheduledExecutor.TimeUnit.MILLISECONDS => value,
            ScheduledExecutor.TimeUnit.SECONDS => value * 1000,
            ScheduledExecutor.TimeUnit.MINUTES => value * 60 * 1000,
            _ => throw new AgenixSystemException("Unsupported time unit")
        };
    }


    public sealed class Builder : AbstractTestActionBuilder<ITestAction, dynamic>
    {
        internal string time = "5000";
        internal ScheduledExecutor.TimeUnit timeUnit = ScheduledExecutor.TimeUnit.MILLISECONDS;

        public static Builder Delay()
        {
            return new Builder();
        }

        public static Builder Sleep()
        {
            return new Builder();
        }

        public Builder Milliseconds(int milliseconds)
        {
            return Time(milliseconds.ToString(), ScheduledExecutor.TimeUnit.MILLISECONDS);
        }

        public Builder Milliseconds(long milliseconds)
        {
            return Time(milliseconds.ToString(), ScheduledExecutor.TimeUnit.MILLISECONDS);
        }

        public Builder Milliseconds(string expression)
        {
            Time(expression, ScheduledExecutor.TimeUnit.MILLISECONDS);
            return this;
        }

        public Builder Seconds(double seconds)
        {
            Milliseconds((int)Math.Round(seconds * 1000));
            return this;
        }

        public Builder Seconds(int seconds)
        {
            return Time((seconds * 1000L).ToString(), ScheduledExecutor.TimeUnit.MILLISECONDS);
        }

        public Builder Seconds(long seconds)
        {
            return Time((seconds * 1000L).ToString(), ScheduledExecutor.TimeUnit.MILLISECONDS);
        }

        public Builder Time(TimeSpan duration)
        {
            Milliseconds(duration.Milliseconds);
            return this;
        }

        public Builder Time(string expression, ScheduledExecutor.TimeUnit timeUnit)
        {
            time = expression;
            this.timeUnit = timeUnit;
            return this;
        }

        public override SleepAction Build()
        {
            return new SleepAction(this);
        }
    }
}
