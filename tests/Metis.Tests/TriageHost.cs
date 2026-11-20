using System.Net.Http.Json;
using System.Text.Json;
using Agentic.Blocks.Testing;
using Metis.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Metis.Tests;

/// <summary>Starts the real API on Kestrel with invented seed data and the default criteria held in memory.</summary>
internal static class TriageHost
{
    // Development-only demo secret used by the in-process host; never a real credential.
    internal const string DemoSecret = "test-only-demo-secret-42";
    internal static readonly DateTimeOffset Now = new(2026, 11, 20, 9, 0, 0, TimeSpan.Zero);

    internal static Dictionary<string, string?> DefaultCriteria() => new()
    {
        ["Scoring:Weights:LoanToValue"] = "30",
        ["Scoring:Weights:DebtServiceCover"] = "20",
        ["Scoring:Weights:SponsorTrackRecord"] = "25",
        ["Scoring:Weights:AssetType"] = "15",
        ["Scoring:Weights:Location"] = "10",
        ["Scoring:LoanToValue:Best"] = "0.55",
        ["Scoring:LoanToValue:Worst"] = "0.80",
        ["Scoring:DebtServiceCover:Best"] = "1.6",
        ["Scoring:DebtServiceCover:Worst"] = "1.0",
        ["Scoring:SponsorDealsForFullMarks"] = "12",
        ["Scoring:AssetTypeScores:Industrial"] = "1.0",
        ["Scoring:AssetTypeScores:Multifamily"] = "0.85",
        ["Scoring:AssetTypeScores:MixedUse"] = "0.6",
        ["Scoring:AssetTypeScores:Retail"] = "0.4",
        ["Scoring:AssetTypeScores:Office"] = "0.35",
        ["Scoring:AssetTypeScores:Hotel"] = "0.3",
        ["Scoring:AssetTypeScores:Land"] = "0.1",
        ["Scoring:LocationScores:Harbourside"] = "1.0",
        ["Scoring:LocationScores:Eastvale"] = "0.8",
        ["Scoring:LocationScores:Millbrook"] = "0.7",
        ["Scoring:LocationScores:Northgate"] = "0.6",
        ["Scoring:LocationScores:OuterRing"] = "0.3",
    };

    internal static async Task<(KestrelTestHost Host, IConfigurationRoot Config)> StartAsync(bool seed = true)
    {
        var clock = new ManualTimeProvider(Now);
        IConfigurationRoot? config = null;
        var host = await KestrelTestHost.StartAsync(
            builder =>
            {
                builder.Configuration.AddInMemoryCollection(DefaultCriteria());
                builder.Configuration["DemoUsers:Password"] = DemoSecret;
                builder.Configuration["Intake:SeedDemoData"] = seed ? "true" : "false";
                builder.Services.AddSingleton<TimeProvider>(clock);
                AppComposition.ConfigureServices(builder);
                config = builder.Configuration;
            },
            AppComposition.Configure);
        return (host, config!);
    }

    internal static async Task<string> LoginAsync(KestrelTestHost host, string email = "analyst@example.com")
    {
        using var response = await host.SendAsync(HttpMethod.Post, "/auth/login", new { email, password = DemoSecret }); // privacy-guard: ignore (test-only constant)
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("accessToken").GetString()!;
    }

    internal static async Task<JsonElement> GetJsonAsync(KestrelTestHost host, string path, string token)
    {
        using var response = await host.SendAsync(HttpMethod.Get, path, bearer: token);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<JsonElement>())!;
    }

    internal static List<string> DealNames(JsonElement queue) =>
        queue.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("dealName").GetString()!).ToList();
}
