# HttpClientUtilities

Utilities for .NET `HttpClient`.

This projects provides utilities to implement resilient and manageable components
that uses `HttpClient`. In particular, it provides:
- a Fluent URI builder
- standard `Options` for configuring `HttpClient` that integrated with [`Polly`](http://www.thepollyproject.org/)
- a `DelegatingHandler` that allows complete tracing of requests and responses, including the body

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

## Getting Started

Install the package:

```
Install-Package HttpClientUtilities
```

Then the various features can be used independently. However, they are designed
to work in conjunction with the [`IHttpClientFactory`](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2).

## Features

### FluentUriBuilder

The `FluentUriBuilder` is designed to help build composable Uri when calling APIs.

Here is an example:

```csharp

using HttpClientUtilities;

FluentUriBuilder
  .ForPath("api")                     // The base Uri
  .WithSegment("users")               // Adds a Uri segment
  .WithParam("username", "John Doe")  // Adds a query string parameter, encoded
  .WithFragment("anchor-point");      // Sets the Uri fragment
  // This will produce "api/users?username=John+Doe#anchor-point."
```

Please refer to the documentation for each method for more information.

The `FluentUriBuilder` is immutable, which means each method returns a new immutable 
instance that can be independently re-used:

```csharp

var baseBuilder = FluentUriBuilder
  .ForPath("api")
  .WithSegment("users");

baseBuilder.WithParam("username", "John Doe"); // "api/users?username=John+Doe"
baseBuilder.WithParam("foo", "bar"); // "api/users?foo=bar"
```

It also implicitly casts to a `System.Uri`:

```csharp
Uri uri = FluentUriBuilder.ForPath("api/users");

// This works as well
HttpClient client;
var response = await client.GetAsync(FluentUriBuilder.ForPath("api/users"));
```

Additional extension methods can be added to the `FluentUriBuilder` class to
provide your own custom behavior:

```csharp
using System;
using System.Globalization;

namespace HttpClientUtilities
{
    public static class FluentUriBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="DateTimeOffset"/> as a query string parameter by formatting it as UTC ISO.
        /// </summary>
        public static FluentUriBuilder WithParam(this FluentUriBuilder buidler, string name, DateTimeOffset date)
            => buidler.WithParam(name, date.UtcDateTime.ToString("o", CultureInfo.InvariantCulture), encode: false);
    }
}
```

### Request/Response Tracing

When configuring the `HttpClient` via the factory, add the tracing handler:

```csharp

using Microsoft.Extensions.DependencyInjection;


services
    .AddHttpClient<MyNamedClient>() // Adds a named HttpClient
    .AddHttpTracing<MyNamedClient>(); // Attaches a named tracing handler.
```

The tracing handler (`AddHttpTracing`) should probably be the last handler in the
chain in order to capture all modifications done by other handlers if they exist.

The logger category [follows the conventions defined by the `IHttpClientFactory`](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2#logging)
by naming the category `System.Net.Http.HttpClient.MyNamedClient.TraceHandler`.

Event ids used for the various messages:

| Event id | Event Name         | Log level | Description                         |
|----------|--------------------|-----------|-------------------------------------|
| 200      | RequestSuccessful  | Trace     | Request trace on successful calls   |
| 201      | RequestError       | Warning   | Request trace on unsuccessful calls |
| 210      | ResponseSuccessful | Trace     | Response on successful calls        |
| 211      | ResponseError      | Warning   | Response on unsuccessful calls      |

A successful call is determined by default using `HttpResponseMessage.IsSuccessStatusCode`.
This can be customized when adding the handler:

```csharp
services
    .AddHttpClient<MyNamedClient>()
    .AddHttpTracing<MyNamedClient>(
        response => response.StatusCode >= HttpStatusCode.InternalServerError);
```

#### Using with Application Insights

By default, Application Insights captures only `Warning` and `Error` log levels.
To enable tracing of successful requests and responses, [configure the log level for Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/ilogger). 
Example within the `appsettings.json` file:

```json
{
  "Logging": {
    "ApplicationInsights": {
      "LogLevel": {
        "System.Net.Http.HttpClient.MyNamedClient.TraceHandler": "Trace"
      }
    },
  }
}
```

### Default options for HTTP and Polly configuration

[Polly](http://www.thepollyproject.org/) is a great library with integration with
the `IHttpClientFactory` and helps tremendously to provide good strategies to handle
transient errors (retries, circuit breaker...).

Unfortunately, by default it does not provide a configuration model that is
integrated with the [ASP.NET Core Configuration model](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2).

This library fills the gap by providing:

- a base class for `HttpOptions` that can be used to specify properties for Polly policies
- helpers extension methods to configure the `HttpClient` automatically

This strategy adds the following configurable behaviors:

- Request timeouts
- Automatic retries, with a [jittered sleep strategy](https://github.com/App-vNext/Polly/wiki/Retry-with-jitter)
- Circuit breaker
- The possibility to restrict the number of requests issued in parallel
- The possibility to add a default `Authorization` header (useful for static API authentication mechanisms)

To use this feature:

1. Create an Options class for your client that derives from `HttpOptions`:

```csharp
using HttpClientUtilities;

public class GitHubServiceOptions : HttpOptions
{
    // Additional configurations goes here if needed.
}
```

2. Create you Component that uses the `HttpClient` ([following the standard `IHttpClientFactory` usage](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2)):

```csharp
public class GitHubService
{
    public GitHubService(HttpClient client)
    {
        // ...
    }
}
```

3. Register the options and client using extension methods:

```csharp
services
    // Configure the options (in this case by loading it from the configuration)
    .Configure<GitHubServiceOptions>(configuration.GetSection(nameof(GitHubServiceOptions)))
    // Register the service
    .AddTransient<GitHubService>()
    // Register the named HttpClient and apply HttpClient configuration
    .AddAndConfigureHttpClient<GitHubService, GitHubServiceOptions>()
    // Adds Polly policies configured in the options
    .AddPoliciesFromOptions<GitHubServiceOptions>();

```

4. Configure the behaviors using the options, for example here in the `appsettings.json`:

```json
{
  "GitHubServiceOptions": {
    "BaseAddress": "https://api.github.com",
    "Timeout": "00:00:05",
    "AuthorizationHeader": "Basic dXNlcm5hbWU6cGFzc3dvcmQ=",
    "NumberOfRetries": 2,
    "RetriesSleepDuration": "00:00:00.100",
    "RetriesMaximumSleepDuration": "00:00:01",
    "ErrorsAllowedBeforeBreaking": 5,
    "BreakDuration": "01:00:00",
    "MaxParallelization": 10
  }
}
```

The following defaults are applied when using `HttpOptions` without overriding the values:

| Parameter                   | Default Value    | Description                                                                                          |
|-----------------------------|------------------|------------------------------------------------------------------------------------------------------|
| Timeout                     | 30 seconds       | The request timeout.                                                                                 |
| NumberOfRetries             | 3                | Number of automatic retries on transient HTTP errors. Set to 0 to disable automatic retries.         |
| RetriesSleepDuration        | 300 milliseconds | Minimum sleep duration between retries.                                                              |
| RetriesMaximumSleepDuration | 3 seconds        | Maximum sleep duration between retries. Set to 00:00:00 to have a fixed RetriesSleepDuration.        |
| ErrorsAllowedBeforeBreaking | 10               | Number of errors to allow before the Circuit Breaker opens. Set to 0 to disable the Circuit Breaker. |
| BreakDuration               | 1 minute         | Duration of a break when the circuit breaker opens.                                                  |
| MaxParallelization          | 0                | Maximum number of parallel requests allowed (in flight). Set to 0 for unlimited parallel requests.   |



## Changelog

Please consult the [CHANGELOG](CHANGELOG.md) for more information about version
history.

## License

This project is licensed under the Apache 2.0 license - see the
[LICENSE](LICENSE) file for details.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on the process for
contributing to this project.

Be mindful of our [Code of Conduct](CODE_OF_CONDUCT.md).

## Acknowledgments

- [Polly](http://www.thepollyproject.org/)
- [Flurl](https://flurl.dev/)
