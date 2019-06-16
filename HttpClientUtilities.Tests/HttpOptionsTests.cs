using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HttpClientUtilities.Tests
{
    public class HttpOptionsTests
    {
        [Fact]
        public async Task ItShouldBuildHttpClient()
        {
            var baseAddress = new Uri("https://example.org");
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddAndConfigureHttpClient<TestOptions>(nameof(ItShouldBuildHttpClient), (_, configureClient, opt) =>
            {
                configureClient.BaseAddress = baseAddress;
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var client = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(ItShouldBuildHttpClient));
            client.BaseAddress.Should().Be(baseAddress);
            client.Timeout.Should().Be(HttpOptions.DefaultTimeout);
            client.DefaultRequestHeaders.UserAgent.First().Product.Name.Should().Contain(
                typeof(HttpOptionsTests).Assembly.GetName().Name);
            client.DefaultRequestHeaders.UserAgent.First().Product.Version.Should().Contain(
                typeof(HttpOptionsTests).Assembly.GetName().Version.ToString());
            client.DefaultRequestHeaders.TryGetValues("X-CustomHeader", out var customHeaderValues).Should().BeTrue();
            customHeaderValues.First().Should().Be("Foo");
        }

        private class TestOptions : HttpOptions
        {
            public TestOptions()
            {
                Headers = new Dictionary<string, string>
                {
                    { "X-CustomHeader", "Foo" },
                };
            }
        }
    }
}
