using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace HttpClientUtilities
{
    /// <summary>
    /// <see cref="HttpRequestMessage"/> extension methods.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Returns all headers as a standard HTTP headers string.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>.</param>
        /// <returns>The headers as HTTP headers string.</returns>
        public static string AllHeadersAsString(this HttpRequestMessage request)
            => string.Join(
                Environment.NewLine,
                request.Headers
                    .Concat(request.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
                    .Select(x => $"{x.Key}: {string.Join(" ", x.Value)}"));
    }
}
