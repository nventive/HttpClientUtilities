using System;
using System.Net.Http;
using HttpClientUtilities;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Polly;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IHttpClientBuilder"/> extension methods.
    /// </summary>
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Configures Polly Policy Handlers using options defined in <typeparamref name="T"/>.
        /// Adds both retry and circuit breaker policies depending on the configured <see cref="HttpOptions"/> values.
        /// </summary>
        /// <typeparam name="T">The option type.</typeparam>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/>.</param>
        /// <returns>The updated <see cref="IHttpClientBuilder"/>.</returns>
        public static IHttpClientBuilder AddPoliciesFromOptions<T>(this IHttpClientBuilder builder)
            where T : HttpOptions, new()
        {
            var options = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<T>>().Value;

            if (options.ErrorsAllowedBeforeBreaking > 0)
            {
                builder = builder.AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(options.ErrorsAllowedBeforeBreaking, options.BreakDuration));
            }

            if (options.NumberOfRetries > 0)
            {
                builder = builder.AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(options.NumberOfRetries, _ => options.RetriesSleepDuration));
            }

            return builder;
        }

        /// <summary>
        /// Adds an additional message handler from the dependency injection container for a named <see cref="HttpClient"/>
        /// AND registers it in the container with <see cref="ServiceLifetime.Transient"/> service lifetime.
        /// </summary>
        /// <typeparam name="THandler">The type of the <see cref="DelegatingHandler"/>.</typeparam>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/>.</param>
        /// <param name="implementationFactory">The factory that creates the service, if any.</param>
        /// <returns>The updated <see cref="IHttpClientBuilder"/>.</returns>
        public static IHttpClientBuilder AddAndRegisterHttpMessageHandler<THandler>(
            this IHttpClientBuilder builder,
            Func<IServiceProvider, THandler> implementationFactory = null)
            where THandler : DelegatingHandler
        {
            if (implementationFactory == null)
            {
                builder.Services.TryAddTransient<THandler>();
            }
            else
            {
                builder.Services.TryAddTransient(implementationFactory);
            }

            return builder.AddHttpMessageHandler<THandler>();
        }
    }
}
