using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace HttpClientUtilities.Tests
{
    public class FluentUriBuilderTests
    {
        public static IEnumerable<object[]> ItShouldBuildAQueryStringData()
        {
            yield return new object[]
            {
                FluentUriBuilder.ForPath(string.Empty),
                string.Empty,
            };

            yield return new object[]
            {
                FluentUriBuilder.ForPath("foo"),
                "foo",
            };

            yield return new object[]
            {
                FluentUriBuilder.ForPath("foo/bar"),
                "foo/bar",
            };

            yield return new object[]
            {
                FluentUriBuilder.ForPath("foo").WithParam("bar", "value with space"),
                "foo?bar=value+with+space",
            };

            yield return new object[]
            {
                FluentUriBuilder.ForPath("foo").WithParam("bar", "value with space", false),
                "foo?bar=value with space",
            };

            yield return new object[]
            {
                FluentUriBuilder.ForPath("foo").WithParam("bar1", "value1").WithParam("bar2", "value2"),
                "foo?bar1=value1&bar2=value2",
            };

            yield return new object[]
            {
                FluentUriBuilder.ForPath("foo").WithSegment(null),
                "foo",
            };

            yield return new object[]
            {
                FluentUriBuilder.ForPath("foo").WithSegment(string.Empty),
                "foo",
            };

            yield return new object[]
            {
                FluentUriBuilder.ForPath("foo").WithSegment("bar"),
                "foo/bar",
            };

            yield return new object[]
            {
                FluentUriBuilder.ForPath("/foo").WithSegment("/bar"),
                "/foo/%2Fbar",
            };

            yield return new object[]
            {
                FluentUriBuilder.ForPath("/foo").WithSegment("/bar", false),
                "/foo/bar",
            };

            yield return new object[]
            {
                FluentUriBuilder.ForPath("foo").WithSegments("bar", "foobar"),
                "foo/bar/foobar",
            };

            yield return new object[]
            {
                FluentUriBuilder.ForPath("foo").WithFragment("bar"),
                "foo#bar",
            };

            yield return new object[]
            {
                FluentUriBuilder.ForPath("api").WithFragment("bar").WithParam("mode", "disabled").WithSegment("foo"),
                "api/foo?mode=disabled#bar",
            };

            yield return new object[]
            {
                FluentUriBuilder.ForPath("api").WithSegment("users").WithParam("username", "John Doe").WithFragment("anchor-point"),
                "api/users?username=John+Doe#anchor-point",
            };
        }

        [Theory]
        [MemberData(nameof(ItShouldBuildAQueryStringData))]
        public void ItShouldBuildAUri(FluentUriBuilder builder, string expected)
        {
            ((string)builder).Should().Be(expected);
            ((Uri)builder).Should().Be(new Uri(expected, UriKind.Relative));
            builder.ToUri(new Uri("https://example.org/")).Should().Be(new Uri(new Uri("https://example.org/"), expected));
        }

        [Fact]
        public void ItShouldBeImmutable()
        {
            var builder = FluentUriBuilder.ForPath("api");
            var builder2 = builder.WithSegment("segment");
            var builder3 = builder2.WithSegment("segment2");
            var builder4 = builder3.WithParam("foo", "bar");
            var builder5 = builder4.WithFragment("fragment");

            var allBuildersStrings = new[] { builder.ToString(), builder2.ToString(), builder3.ToString(), builder4.ToString(), builder5.ToString() };
            allBuildersStrings.Distinct().Should().HaveSameCount(allBuildersStrings);
        }
    }
}
