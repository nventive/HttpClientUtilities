using System;
using System.Net.Http;
using HttpClientUtilities;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
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

        /// <summary>
        /// Adds and register <see cref="HttpTracingDelegatingHandler{T}"/> to enable tracing of requests/responses.
        /// </summary>
        /// <typeparam name="TClient">
        /// The type of the typed client.
        /// Will determine the category of the logger (System.Net.Http.HttpClient.T.TraceHandler).
        /// </typeparam>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/>.</param>
        /// <param name="isResponseSuccessful">
        /// A function to allow customization of the evaluation of a successful response.
        /// Defaults to <see cref="HttpResponseMessage.IsSuccessStatusCode"/>.
        /// </param>
        /// <returns>The updated <see cref="IHttpClientBuilder"/>.</returns>
        public static IHttpClientBuilder AddHttpTracing<TClient>(this IHttpClientBuilder builder, Func<HttpResponseMessage, bool> isResponseSuccessful = null)
        {
            if (isResponseSuccessful == null)
            {
                return builder.AddAndRegisterHttpMessageHandler<HttpTracingDelegatingHandler<TClient>>();
            }

            return builder.AddAndRegisterHttpMessageHandler(
                sp => new HttpTracingDelegatingHandler<TClient>(sp.GetRequiredService<ILoggerFactory>(), isResponseSuccessful));
        }
    }
}
