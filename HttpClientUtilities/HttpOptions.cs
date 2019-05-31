using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace HttpClientUtilities
{
    /// <summary>
    /// Base class for options related to <see cref="HttpClient"/>.
    /// </summary>
    public abstract class HttpOptions
    {
        /// <summary>
        /// Gets the default <see cref="Timeout"/> (10 seconds).
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets the default <see cref="NumberOfRetries"/> (3).
        /// </summary>
        public static readonly int DefaultNumberOfRetries = 3;

        /// <summary>
        /// Gets the default <see cref="RetriesSleepDuration"/> (500 ms).
        /// </summary>
        public static readonly TimeSpan DefaultRetriesSleepDuration = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Gets the default <see cref="ErrorsAllowedBeforeBreaking"/> (10).
        /// </summary>
        public static readonly int DefaultErrorsAllowedBeforeBreaking = 10;

        /// <summary>
        /// Gets the default <see cref="BreakDuration"/> (1 minute).
        /// </summary>
        public static readonly TimeSpan DefaultBreakDuration = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets the <see cref="HttpClient.BaseAddress"/> value.
        /// </summary>
        public Uri BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="HttpClient.Timeout"/> value.
        /// </summary>
        public TimeSpan Timeout { get; set; } = DefaultTimeout;

        /// <summary>
        /// Gets or sets the number of automatic retries in case of transient HTTP failures.
        /// Set to 0 to disable automatic retries.
        /// </summary>
        public int NumberOfRetries { get; set; } = DefaultNumberOfRetries;

        /// <summary>
        /// Gets or sets the sleep duration in-between retries in case of transient HTTP failures.
        /// </summary>
        public TimeSpan RetriesSleepDuration { get; set; } = DefaultRetriesSleepDuration;

        /// <summary>
        /// Gets or sets the number of errors to allow before the Circuit Breaker opens.
        /// Set to 0 to disable the Circuit Breaker.
        /// </summary>
        public int ErrorsAllowedBeforeBreaking { get; set; } = DefaultErrorsAllowedBeforeBreaking;

        /// <summary>
        /// Gets or sets the duration of a break when the circuit breaker opens.
        /// </summary>
        public TimeSpan BreakDuration { get; set; } = DefaultBreakDuration;
    }
}
