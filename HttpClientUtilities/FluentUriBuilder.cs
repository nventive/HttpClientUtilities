using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace HttpClientUtilities
{
    /// <summary>
    /// Helper class to build query strings for HttpClient.
    /// </summary>
    /// <example>
    /// FluentUriBuilder
    ///     .ForPath("api")
    ///     .WithSegment("users")
    ///     .WithParam("username", "John Doe")
    ///     .WithFragment("anchor-point")
    ///     .ToString(); -> api/users?username=John+Doe#anchor-point.
    /// </example>
    public class FluentUriBuilder
    {
        private readonly string _path;
        private readonly NameValueCollection _parameters;
        private readonly string _fragment;

        private FluentUriBuilder(string path, NameValueCollection parameters, string fragment)
        {
            _path = path;
            _parameters = parameters;
            _fragment = fragment;
        }

        /// <summary>
        /// Implicit conversion to string.
        /// </summary>
        /// <param name="builder">The <see cref="FluentUriBuilder"/> to convert.</param>
        public static implicit operator string(FluentUriBuilder builder)
        {
            return builder?.ToString();
        }

        /// <summary>
        /// Implicit conversion to <see cref="Uri"/>.
        /// </summary>
        /// <param name="builder">The <see cref="FluentUriBuilder"/> to convert.</param>
        public static implicit operator Uri(FluentUriBuilder builder)
        {
            return builder?.ToUri();
        }

        /// <summary>
        /// Creates a new instance of <see cref="FluentUriBuilder"/> using
        /// <paramref name="path"/> as the base path.
        /// </summary>
        /// <param name="path">The Uri Path.</param>
        /// <returns>A new <see cref="FluentUriBuilder"/>.</returns>
        public static FluentUriBuilder ForPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return new FluentUriBuilder(path, new NameValueCollection(), null);
        }

        /// <summary>
        /// Adds a path segment.
        /// </summary>
        /// <param name="segment">The new segment to add.</param>
        /// <param name="encode">True to UrlEncode the value, false otherwise.</param>
        /// <returns>A new instance of <see cref="FluentUriBuilder"/> with the added segment.</returns>
        public FluentUriBuilder WithSegment(string segment, bool encode = true)
        {
            var newPath = _path;
            if (string.IsNullOrEmpty(segment))
            {
                return this;
            }

            if (encode)
            {
                segment = WebUtility.UrlEncode(segment);
            }

            return new FluentUriBuilder(
                string.Format(CultureInfo.InvariantCulture, "{0}/{1}", newPath.TrimEnd('/'), segment.TrimStart('/')),
                _parameters,
                _fragment);
        }

        /// <summary>
        /// Adds a path segment. Calls <see cref="object.ToString"/> on the <paramref name="segment"/>.
        /// </summary>
        /// <param name="segment">The new segment to add.</param>
        /// <param name="encode">True to UrlEncode the value, false otherwise.</param>
        /// <returns>A new instance of <see cref="FluentUriBuilder"/> with the added segment.</returns>
        public FluentUriBuilder WithSegment(object segment, bool encode = true)
        {
            if (segment == null)
            {
                return this;
            }

            return WithSegment(segment.ToString(), encode);
        }

        /// <summary>
        /// Adds and encodes multiple path segments.
        /// </summary>
        /// <param name="segments">The new segments to add. They will be URL encoded.</param>
        /// <returns>A new instance of <see cref="FluentUriBuilder"/> with the added path segments.</returns>
        public FluentUriBuilder WithSegments(params string[] segments)
        {
            var result = this;
            foreach (var segment in segments)
            {
                result = result.WithSegment(segment);
            }

            return result;
        }

        /// <summary>
        /// Adds a query string parameter.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="encode">True to UrlEncode the value, false otherwise.</param>
        /// <returns>A new instance of <see cref="FluentUriBuilder"/> with the added parameter.</returns>
        public FluentUriBuilder WithParam(string name, string value, bool encode = true)
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
            return new FluentUriBuilder(_path, newParameters, _fragment);
        }

        /// <summary>
        /// Adds a query string parameter. Calls <see cref="object.ToString"/> on the <paramref name="value"/>.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="encode">True to UrlEncode the value, false otherwise.</param>
        /// <returns>A new instance of <see cref="FluentUriBuilder"/> with the added parameter.</returns>
        public FluentUriBuilder WithParam(string name, object value, bool encode = true)
        {
            if (value == null)
            {
                return this;
            }

            return WithParam(name, value.ToString(), encode);
        }

        /// <summary>
        /// Conditionaly adds a query string parameter.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="condition">The condition to check. The param will only be added if true.</param>
        /// <param name="encode">True to UrlEncode the value, false otherwise.</param>
        /// <returns>A new instance of <see cref="FluentUriBuilder"/> with the added parameter.</returns>
        public FluentUriBuilder WithParamIf(string name, string value, bool condition, bool encode = true)
        {
            if (!condition)
            {
                return this;
            }

            return WithParam(name, value, encode);
        }

        /// <summary>
        /// Conditionaly adds a query string parameter. Calls <see cref="object.ToString"/> on the <paramref name="value"/>.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="condition">The condition to check. The param will only be added if true.</param>
        /// <param name="encode">True to UrlEncode the value, false otherwise.</param>
        /// <returns>A new instance of <see cref="FluentUriBuilder"/> with the added parameter.</returns>
        public FluentUriBuilder WithParamIf(string name, object value, bool condition, bool encode = true)
        {
            if (!condition)
            {
                return this;
            }

            return WithParam(name, value, encode);
        }

        /// <summary>
        /// Sets the fragment (anything after #).
        /// </summary>
        /// <param name="fragment">The fragment to set.</param>
        /// <returns>A new instance of <see cref="FluentUriBuilder"/> with the fragment set.</returns>
        public FluentUriBuilder WithFragment(string fragment)
        {
            return new FluentUriBuilder(_path, _parameters, fragment);
        }

        /// <summary>
        /// Returns the path and query string.
        /// </summary>
        /// <returns>A string that represents the Path and Query String parameters.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder(_path);

            if (_parameters.Count > 0)
            {
                stringBuilder.Append("?");
                stringBuilder.Append(
                    string.Join(
                        "&",
                        _parameters
                            .AllKeys
                            .SelectMany(key =>
                                _parameters
                                    .GetValues(key)
                                    .Select(value => $"{key}={value}")
                                    .ToArray())));
            }

            if (!string.IsNullOrEmpty(_fragment))
            {
                stringBuilder.Append($"#{_fragment}");
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns a <see cref="Uri"/>.
        /// </summary>
        /// <returns>A new <see cref="Uri"/>.</returns>
        public Uri ToUri() => new Uri(ToString(), UriKind.RelativeOrAbsolute);

        /// <summary>
        /// Returns a <see cref="Uri"/>.
        /// </summary>
        /// <param name="baseUri">An absolute <see cref="Uri"/> that is the base for the new <see cref="Uri"/> instance.</param>
        /// <returns>A new <see cref="Uri"/>.</returns>
        public Uri ToUri(Uri baseUri) => new Uri(baseUri, ToUri());
    }
}
