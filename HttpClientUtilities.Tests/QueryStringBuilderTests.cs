using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace HttpClientUtilities.Tests
{
    public class QueryStringBuilderTests
    {
        public static IEnumerable<object[]> ItShouldBuildAQueryStringData()
        {
            yield return new object[]
            {
                QueryStringBuilder.For(string.Empty),
                string.Empty,
            };

            yield return new object[]
            {
                QueryStringBuilder.For("foo"),
                "foo",
            };

            yield return new object[]
            {
                QueryStringBuilder.For("foo/bar"),
                "foo/bar",
            };

            yield return new object[]
            {
                QueryStringBuilder.For("foo").AddParam("bar", "value with space"),
                "foo?bar=value+with+space",
            };

            yield return new object[]
            {
                QueryStringBuilder.For("foo").AddParam("bar", "value with space", false),
                "foo?bar=value with space",
            };

            yield return new object[]
            {
                QueryStringBuilder.For("foo").AddParam("bar1", "value1").AddParam("bar2", "value2"),
                "foo?bar1=value1&bar2=value2",
            };

            yield return new object[]
            {
                QueryStringBuilder.For("foo").AddParam("bar1", "value1").AddParam("bar1", "value2"),
                "foo?bar1=value1&bar1=value2",
            };
        }

        [Theory]
        [MemberData(nameof(ItShouldBuildAQueryStringData))]
        public void ItShouldBuildAQueryString(QueryStringBuilder builder, string expected)
        {
            builder.ToString().Should().Be(expected);
            builder.ToUri().Should().Be(new Uri(expected, UriKind.Relative));
        }
    }
}
