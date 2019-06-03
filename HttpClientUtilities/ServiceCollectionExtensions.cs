using System;
using System.Net.Http;
using System.Net.Http.Headers;
using HttpClientUtilities;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/>
        /// and configures a binding between the <typeparamref name="TClient"/> type and a named <see cref="HttpClient"/>.
        /// The client name will be set to the type name of <typeparamref name="TClient"/>.
        /// Automatically configure options defined in <typeparamref name="TOptions"/>
        /// (<see cref="HttpOptions.BaseAddress"/>, <see cref="HttpOptions.Timeout"/> and <see cref="HttpOptions.AuthorizationHeader"/>).
        /// </summary>
        /// <typeparam name="TClient">
        /// The type of the typed client. They type specified will be registered in the service
        /// collection as a transient service. See <see cref="Http.ITypedHttpClientFactory{TClient}"/>
        /// for more details about authoring typed clients.</typeparam>
        /// <typeparam name="TImplementation">
        /// The implementation type of the typed client. They type specified will be instantiated
        /// <see cref="Http.ITypedHttpClientFactory{TClient}"/>.
        /// </typeparam>
        /// <typeparam name="TOptions">The <see cref="HttpOptions"/> type to use.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configureClient">A delegate that is used to further configure an <see cref="HttpClient"/>.</param>
        /// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
        public static IHttpClientBuilder AddAndConfigureHttpClient<TClient, TImplementation, TOptions>(
            this IServiceCollection services, Action<IServiceProvider, HttpClient, TOptions> configureClient = null)
            where TClient : class
            where TImplementation : class, TClient
            where TOptions : HttpOptions, new() =>
                services.AddHttpClient<TClient, TImplementation>((sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<TOptions>>().Value;
                    client.BaseAddress = options.BaseAddress;
                    client.Timeout = options.Timeout;
                    if (!string.IsNullOrEmpty(options.AuthorizationHeader))
                    {
                        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(options.AuthorizationHeader);
                    }

                    configureClient?.Invoke(sp, client, options);
                });
    }
}
