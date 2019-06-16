using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HttpClientUtilities
{
    /// <summary>
    /// Base class for options related to <see cref="HttpClient"/>.
    /// </summary>
    public abstract class HttpOptions
    {
        /// <summary>
        /// Gets the default <see cref="Timeout"/> (30 seconds).
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets the default <see cref="NumberOfRetries"/> (3).
        /// </summary>
        public static readonly int DefaultNumberOfRetries = 3;

        /// <summary>
        /// Gets the default <see cref="RetriesSleepDuration"/> (300 ms).
        /// </summary>
        public static readonly TimeSpan DefaultRetriesSleepDuration = TimeSpan.FromMilliseconds(300);

        /// <summary>
        /// Gets the default <see cref="RetriesMaximumSleepDuration"/> (3 seconds).
        /// </summary>
        public static readonly TimeSpan DefaultRetriesMaximumSleepDuration = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Gets the default <see cref="ErrorsAllowedBeforeBreaking"/> (10).
        /// </summary>
        public static readonly int DefaultErrorsAllowedBeforeBreaking = 10;

        /// <summary>
        /// Gets the default <see cref="BreakDuration"/> (1 minute).
        /// </summary>
        public static readonly TimeSpan DefaultBreakDuration = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets the default <see cref="MaxParallelization"/>. (0 = disabled).
        /// </summary>
        public static readonly int DefaultMaxParallelization = 0;

        private IDictionary<string, string> _headers;

        /// <summary>
        /// Gets or sets the <see cref="HttpClient.BaseAddress"/> value.
        /// </summary>
        public Uri BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="HttpClient.Timeout"/> value.
        /// </summary>
        public TimeSpan Timeout { get; set; } = DefaultTimeout;

        /// <summary>
        /// Gets or sets the default headers.
        /// </summary>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Headers will never be null and allow binding from configuraiton.")]
        public IDictionary<string, string> Headers
        {
            get => _headers ?? (_headers = new Dictionary<string, string>());
            set => _headers = value;
        }

        /// <summary>
        /// Gets or sets the number of automatic retries in case of transient HTTP failures.
        /// Set to 0 to disable automatic retries.
        /// </summary>
        public int NumberOfRetries { get; set; } = DefaultNumberOfRetries;

        /// <summary>
        /// Gets or sets the sleep duration in-between retries in case of transient HTTP failures.
        /// This is the minimum sleep duration that will be applied in case of Jitter retries.
        /// </summary>
        public TimeSpan RetriesSleepDuration { get; set; } = DefaultRetriesSleepDuration;

        /// <summary>
        /// Gets or sets the maximum sleep duration in-between retries in case of transient HTTP failures.
        /// Set to 00:00:00 to disable Jittered retries and have a constant retry sleep duration applied (using <see cref="RetriesSleepDuration"/>).
        /// </summary>
        public TimeSpan RetriesMaximumSleepDuration { get; set; } = DefaultRetriesMaximumSleepDuration;

        /// <summary>
        /// Gets or sets the number of errors to allow before the Circuit Breaker opens.
        /// Set to 0 to disable the Circuit Breaker.
        /// </summary>
        public int ErrorsAllowedBeforeBreaking { get; set; } = DefaultErrorsAllowedBeforeBreaking;

        /// <summary>
        /// Gets or sets the duration of a break when the circuit breaker opens.
        /// </summary>
        public TimeSpan BreakDuration { get; set; } = DefaultBreakDuration;

        /// <summary>
        /// Gets or sets the maximum number of parallel calls allowed.
        /// Set to 0 for unlimited parallel requests.
        /// </summary>
        public int MaxParallelization { get; set; } = DefaultMaxParallelization;

        /// <summary>
        /// Apply options to the <paramref name="client"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="client">The <see cref="HttpClient"/> to configure.</param>
        public virtual void Apply(IServiceProvider serviceProvider, HttpClient client)
        {
            client.BaseAddress = BaseAddress;
            client.Timeout = Timeout;
            foreach (var header in Headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (client.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                var assemblyName = GetType().Assembly.GetName();
                var hostingEnvironment = serviceProvider.GetService<IHostingEnvironment>();
                if (hostingEnvironment != null)
                {
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd($"{assemblyName.Name}/{assemblyName.Version} ({hostingEnvironment.EnvironmentName})");
                }
                else
                {
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd($"{assemblyName.Name}/{assemblyName.Version}");
                }
            }
        }
    }
}
