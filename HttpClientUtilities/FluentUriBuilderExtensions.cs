namespace HttpClientUtilities
{
    /// <summary>
    /// <see cref="FluentUriBuilder"/> extension methods.
    /// </summary>
    public static class FluentUriBuilderExtensions
    {
        /// <summary>
        /// Adds a path segment. Calls <see cref="object.ToString"/> on the <paramref name="segment"/>.
        /// </summary>
        /// <param name="builder">The <see cref="FluentUriBuilder"/>.</param>
        /// <param name="segment">The new segment to add.</param>
        /// <param name="encode">True to UrlEncode the value, false otherwise.</param>
        /// <returns>A new instance of <see cref="FluentUriBuilder"/> with the added segment.</returns>
        public static FluentUriBuilder WithSegment(this FluentUriBuilder builder, object segment, bool encode = true)
        {
            if (segment == null)
            {
                return builder;
            }

            return builder.WithSegment(segment.ToString(), encode);
        }

        /// <summary>
        /// Conditionally adds a path segment.
        /// </summary>
        /// <param name="builder">The <see cref="FluentUriBuilder"/>.</param>
        /// <param name="segment">The new segment to add.</param>
        /// <param name="condition">The condition to check. The param will only be added if true.</param>
        /// <param name="encode">True to UrlEncode the value, false otherwise.</param>
        /// <returns>A new instance of <see cref="FluentUriBuilder"/> with the added segment.</returns>
        public static FluentUriBuilder WithSegmentIf(this FluentUriBuilder builder, string segment, bool condition, bool encode = true)
        {
            if (!condition)
            {
                return builder;
            }

            return builder.WithSegment(segment, encode);
        }

        /// <summary>
        /// Adds and encodes multiple path segments.
        /// </summary>
        /// <param name="builder">The <see cref="FluentUriBuilder"/>.</param>
        /// <param name="segments">The new segments to add. They will be URL encoded.</param>
        /// <returns>A new instance of <see cref="FluentUriBuilder"/> with the added path segments.</returns>
        public static FluentUriBuilder WithSegments(this FluentUriBuilder builder, params string[] segments)
        {
            var result = builder;
            foreach (var segment in segments)
            {
                result = result.WithSegment(segment);
            }

            return result;
        }

        /// <summary>
        /// Adds a query string parameter. Calls <see cref="object.ToString"/> on the <paramref name="value"/>.
        /// </summary>
        /// <param name="builder">The <see cref="FluentUriBuilder"/>.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="encode">True to UrlEncode the value, false otherwise.</param>
        /// <returns>A new instance of <see cref="FluentUriBuilder"/> with the added parameter.</returns>
        public static FluentUriBuilder WithParam(this FluentUriBuilder builder, string name, object value, bool encode = true)
        {
            if (value == null)
            {
                return builder;
            }

            return builder.WithParam(name, value.ToString(), encode);
        }

        /// <summary>
        /// Conditionally adds a query string parameter.
        /// </summary>
        /// <param name="builder">The <see cref="FluentUriBuilder"/>.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="condition">The condition to check. The param will only be added if true.</param>
        /// <param name="encode">True to UrlEncode the value, false otherwise.</param>
        /// <returns>A new instance of <see cref="FluentUriBuilder"/> with the added parameter.</returns>
        public static FluentUriBuilder WithParamIf(this FluentUriBuilder builder, string name, string value, bool condition, bool encode = true)
        {
            if (!condition)
            {
                return builder;
            }

            return builder.WithParam(name, value, encode);
        }

        /// <summary>
        /// Conditionally adds a query string parameter. Calls <see cref="object.ToString"/> on the <paramref name="value"/>.
        /// </summary>
        /// <param name="builder">The <see cref="FluentUriBuilder"/>.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="condition">The condition to check. The param will only be added if true.</param>
        /// <param name="encode">True to UrlEncode the value, false otherwise.</param>
        /// <returns>A new instance of <see cref="FluentUriBuilder"/> with the added parameter.</returns>
        public static FluentUriBuilder WithParamIf(this FluentUriBuilder builder, string name, object value, bool condition, bool encode = true)
        {
            if (!condition)
            {
                return builder;
            }

            return builder.WithParam(name, value, encode);
        }

        /// <summary>
        /// Conditionally sets the fragment (anything after #).
        /// </summary>
        /// <param name="builder">The <see cref="FluentUriBuilder"/>.</param>
        /// <param name="fragment">The fragment to set.</param>
        /// <param name="condition">The condition to check. The segment will only be added if true.</param>
        /// <returns>A new instance of <see cref="FluentUriBuilder"/> with the fragment set.</returns>
        public static FluentUriBuilder WithFragmentIf(this FluentUriBuilder builder, string fragment, bool condition)
        {
            if (!condition)
            {
                return builder;
            }

            return builder.WithFragment(fragment);
        }
    }
}
