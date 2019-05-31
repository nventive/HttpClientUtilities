using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;

namespace HttpClientUtilities
{
    /// <summary>
    /// Helper class to build query strings for HttpClient.
    /// </summary>
    /// <example>
    /// QueryStringBuilder
    ///     .For("clients")
    ///     .AddParam("name", "toto")
    ///     .ToString(); -> clients?name=toto.
    /// </example>
    public class QueryStringBuilder
    {
        private readonly string _path;
        private readonly NameValueCollection _parameters;

        private QueryStringBuilder(string path, NameValueCollection parameters = null)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _parameters = parameters ?? new NameValueCollection();
        }

        /// <summary>
        /// Implicit conversion to string.
        /// </summary>
        /// <param name="queryStringBuilder">The <see cref="QueryStringBuilder"/> to convert.</param>
        public static implicit operator string(QueryStringBuilder queryStringBuilder)
        {
            return queryStringBuilder?.ToString();
        }

        /// <summary>
        /// Implicit conversion to <see cref="Uri"/>.
        /// </summary>
        /// <param name="queryStringBuilder">The <see cref="QueryStringBuilder"/> to convert.</param>
        public static implicit operator Uri(QueryStringBuilder queryStringBuilder)
        {
            return queryStringBuilder?.ToUri();
        }

        /// <summary>
        /// Creates a new instance of <see cref="QueryStringBuilder"/> using
        /// <paramref name="path"/> as the base path.
        /// </summary>
        /// <param name="path">The Uri Path.</param>
        /// <returns>A new <see cref="QueryStringBuilder"/>.</returns>
        public static QueryStringBuilder For(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return new QueryStringBuilder(path);
        }

        /// <summary>
        /// Adds a query string parameter.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="encode">True to UrlEncode the value, false otherwise.</param>
        /// <returns>A new instance of <see cref="QueryStringBuilder"/> with the added parameter.</returns>
        public QueryStringBuilder AddParam(string name, string value, bool encode = true)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(value))
            {
                return this;
            }

            var newParameters = new NameValueCollection(_parameters.Count + 1, _parameters)
            {
                { name, encode ? WebUtility.UrlEncode(value) : value },
            };
            return new QueryStringBuilder(_path, newParameters);
        }

        /// <summary>
        /// Adds a query string parameter. Calls <see cref="object.ToString"/> on the <paramref name="value"/>.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="encode">True to UrlEncode the value, false otherwise.</param>
        /// <returns>A new instance of <see cref="QueryStringBuilder"/> with the added parameter.</returns>
        public QueryStringBuilder AddParam(string name, object value, bool encode = true)
        {
            if (value == null)
            {
                return this;
            }

            return AddParam(name, value.ToString(), encode);
        }

        /// <summary>
        /// Returns the path and query string.
        /// </summary>
        /// <returns>A string that represents the Path and Query String parameters.</returns>
        public override string ToString()
        {
            if (_parameters.Count == 0)
            {
                return _path;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}?{1}",
                _path,
                string.Join(
                    "&",
                    _parameters
                        .AllKeys
                        .SelectMany(key =>
                            _parameters
                                .GetValues(key)
                                .Select(value => string.Format(CultureInfo.InvariantCulture, "{0}={1}", key, value))
                                .ToArray())));
        }

        /// <summary>
        /// Returns the path and query string as a <see cref="UriKind.Relative"/> <see cref="Uri"/>.
        /// </summary>
        /// <returns>A new <see cref="Uri"/>.</returns>
        public Uri ToUri() => new Uri(ToString(), UriKind.Relative);
    }
}
