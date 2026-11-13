using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CarnetDeSalle.Api.Tests;

public class HealthEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Health_repond_Healthy()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Version_renvoie_la_version_configuree()
    {
        var client = factory.CreateClient();

        var version = await client.GetFromJsonAsync<VersionInfo>("/api/version", TestContext.Current.CancellationToken);

        Assert.NotNull(version);
        Assert.False(string.IsNullOrWhiteSpace(version.Version));
    }
}
