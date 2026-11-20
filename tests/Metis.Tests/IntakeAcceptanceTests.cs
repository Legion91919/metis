using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Metis.Tests;

public class IntakeAcceptanceTests
{
    private static object NewApplication(Guid id, string dealName = "Saltpan Wharf Sheds") => new
    {
        id,
        dealName,
        sponsor = "Morrow Quay Partners",
        assetType = "Industrial",
        region = "Harbourside",
        loanAmount = 2_500_000m,
        propertyValue = 5_000_000m,
        currency = "GBP",
        debtServiceCover = 1.65m,
        sponsorCompletedDeals = 12,
    };

    [Fact]
    public async Task A_retried_submission_is_replayed_and_a_changed_body_with_the_same_id_is_a_conflict()
    {
        var (host, _) = await TriageHost.StartAsync();
        await using var _host = host;
        var token = await TriageHost.LoginAsync(host);
        var id = Guid.Parse("6d1f6f0e-0b51-4c39-9a43-3e3c0f6b2a11");

        using var created = await host.SendAsync(HttpMethod.Post, "/api/applications", NewApplication(id), token);
        using var replayed = await host.SendAsync(HttpMethod.Post, "/api/applications", NewApplication(id), token);
        using var conflict = await host.SendAsync(HttpMethod.Post, "/api/applications", NewApplication(id, "Another Name"), token);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(HttpStatusCode.OK, replayed.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        var queue = await TriageHost.GetJsonAsync(host, "/api/queue", token);
        Assert.Equal(11, queue.GetProperty("total").GetInt32());
        Assert.Equal(1, TriageHost.DealNames(queue).Count(n => n == "Saltpan Wharf Sheds"));
    }

    [Fact]
    public async Task A_new_application_is_scored_with_the_current_weights()
    {
        var (host, _) = await TriageHost.StartAsync(seed: false);
        await using var _host = host;
        var token = await TriageHost.LoginAsync(host);

        using var created = await host.SendAsync(HttpMethod.Post, "/api/applications", NewApplication(Guid.NewGuid()), token);
        var body = await created.Content.ReadFromJsonAsync<JsonElement>();
        using var fetched = await host.SendAsync(HttpMethod.Get, $"/api/applications/{body.GetProperty("id").GetGuid()}", bearer: token);

        Assert.Equal(HttpStatusCode.OK, fetched.StatusCode);
        var queue = await TriageHost.GetJsonAsync(host, "/api/queue", token);
        Assert.Equal(100.0m, queue.GetProperty("items")[0].GetProperty("score").GetDecimal());
    }

    [Fact]
    public async Task An_invalid_application_returns_field_errors()
    {
        var (host, _) = await TriageHost.StartAsync();
        await using var _host = host;
        var token = await TriageHost.LoginAsync(host);

        using var response = await host.SendAsync(HttpMethod.Post, "/api/applications",
            new { id = Guid.Empty, dealName = "", sponsor = "x", assetType = "Castle", region = "Harbourside", loanAmount = -1m, propertyValue = 0m, currency = "GBP", debtServiceCover = 1.2m, sponsorCompletedDeals = 1 },
            token);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.Contains("assetType", text);
        Assert.Contains("loanAmount", text);
        Assert.Contains("id", text);
    }

    [Fact]
    public async Task Unknown_application_is_not_found()
    {
        var (host, _) = await TriageHost.StartAsync();
        await using var _host = host;
        var token = await TriageHost.LoginAsync(host);

        using var response = await host.SendAsync(HttpMethod.Get, $"/api/applications/{Guid.NewGuid()}", bearer: token);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Health_endpoints_answer()
    {
        var (host, _) = await TriageHost.StartAsync();
        await using var _host = host;

        using var live = await host.SendAsync(HttpMethod.Get, "/health/live");
        using var ready = await host.SendAsync(HttpMethod.Get, "/health/ready");

        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
        Assert.Equal(HttpStatusCode.OK, ready.StatusCode);
    }
}
