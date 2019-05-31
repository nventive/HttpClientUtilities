using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HttpClientUtilities
{
    /// <summary>
    /// Helper class that manages <see cref="ILogger"/> events for <see cref="HttpTracingDelegatingHandler"/>.
    /// </summary>
    public static class HttpTracingDelegatingHandlerLoggerExtensions
    {
        private const string RequestMessageFormat = @"
{RequestMethod} {RequestUri} {RequestHttpVersion}
{RequestHeaders}
{RequestBody}";

        private const string ResponseMessageFormat = @"
{ResponseHttpVersion} {ResponseStatusCode} {ResponseReason}
{ResponseHeaders}
{ResponseBody}";

        private static readonly Action<ILogger, string, string, string, string, string, Exception> _requestSuccessful =
            LoggerMessage.Define<string, string, string, string, string>(
                LogLevel.Trace,
                new EventId(200, "RequestSuccessful"),
                RequestMessageFormat);

        private static readonly Action<ILogger, string, string, string, string, string, Exception> _requestError =
            LoggerMessage.Define<string, string, string, string, string>(
                LogLevel.Warning,
                new EventId(201, "RequestError"),
                RequestMessageFormat);

        private static readonly Action<ILogger, string, string, string, string, string, Exception> _responseSuccessful =
            LoggerMessage.Define<string, string, string, string, string>(
                LogLevel.Trace,
                new EventId(210, "ResponseSuccessful"),
                ResponseMessageFormat);

        private static readonly Action<ILogger, string, string, string, string, string, Exception> _responseError =
            LoggerMessage.Define<string, string, string, string, string>(
                LogLevel.Warning,
                new EventId(211, "ResponseError"),
                ResponseMessageFormat);

        /// <summary>
        /// Traces the full <paramref name="request"/> when the interaction is sucessful.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task RequestSuccessful(this ILogger logger, HttpRequestMessage request)
        {
            _requestSuccessful(
                logger,
                request.Method.ToString(),
                request.RequestUri.ToString(),
                $"HTTP/{request.Version}",
                request.AllHeadersAsString(),
                request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync(),
                null);
        }

        /// <summary>
        /// Traces the full <paramref name="request"/> when the interaction is on error.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/>.</param>
        /// <param name="ex">The <see cref="Exception"/> if any.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task RequestError(this ILogger logger, HttpRequestMessage request, Exception ex = null)
        {
            _requestError(
                logger,
                request.Method.ToString(),
                request.RequestUri.ToString(),
                $"HTTP/{request.Version}",
                request.AllHeadersAsString(),
                request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync(),
                ex);
        }

        /// <summary>
        /// Traces the full <paramref name="response"/> when the interaction is sucessful.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task ResponseSuccessful(this ILogger logger, HttpResponseMessage response)
        {
            _responseSuccessful(
                logger,
                $"HTTP/{response.Version}",
                ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture),
                response.ReasonPhrase,
                response.AllHeadersAsString(),
                response.Content == null ? string.Empty : await response.Content.ReadAsStringAsync(),
                null);
        }

        /// <summary>
        /// Traces the full <paramref name="response"/> when the interaction is on error.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <param name="ex">The <see cref="Exception"/> if any.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task ResponseError(this ILogger logger, HttpResponseMessage response, Exception ex = null)
        {
            _responseError(
                logger,
                $"HTTP/{response.Version}",
                ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture),
                response.ReasonPhrase,
                response.AllHeadersAsString(),
                response.Content == null ? string.Empty : await response.Content.ReadAsStringAsync(),
                ex);
        }
    }
}
