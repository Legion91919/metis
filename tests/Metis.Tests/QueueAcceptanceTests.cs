using System.Net;
using System.Text.Json;

namespace Metis.Tests;

/// <summary>Slice 1 acceptance criteria, one test per criterion, over real HTTP with seeded invented data.</summary>
public class QueueAcceptanceTests
{
    [Fact]
    public async Task AC1_given_weights_in_config_when_deals_are_scored_then_the_order_is_deterministic()
    {
        var (host, _) = await TriageHost.StartAsync();
        await using var _host = host;
        var token = await TriageHost.LoginAsync(host);

        var first = await TriageHost.GetJsonAsync(host, "/api/queue", token);
        var second = await TriageHost.GetJsonAsync(host, "/api/queue", token);

        Assert.Equal(first.GetRawText(), second.GetRawText());
        var names = TriageHost.DealNames(first);
        Assert.Equal(10, names.Count);
        Assert.Equal("Pier 7 Cold Store", names[0]);
        Assert.Equal("Weir Street Retail", names[^1]);
        var scores = first.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("score").GetDecimal()).ToList();
        Assert.Equal(scores.OrderByDescending(s => s).ToList(), scores);
        Assert.Equal(95.8m, scores[0]);
    }

    [Fact]
    public async Task AC2_given_a_deal_then_the_reason_names_the_top_three_criteria_that_moved_its_score()
    {
        var (host, _) = await TriageHost.StartAsync();
        await using var _host = host;
        var token = await TriageHost.LoginAsync(host);

        var queue = await TriageHost.GetJsonAsync(host, "/api/queue", token);

        foreach (var item in queue.GetProperty("items").EnumerateArray())
        {
            var movers = item.GetProperty("movers").EnumerateArray().ToList();
            Assert.Equal(3, movers.Count);
            var explanation = item.GetProperty("explanation").GetString()!;
            Assert.DoesNotContain("\n", explanation);
            foreach (var mover in movers)
            {
                Assert.Contains(mover.GetProperty("criterion").GetString()!.ToUpperInvariant(), explanation.ToUpperInvariant());
            }
        }

        var top = queue.GetProperty("items")[0];
        var names = top.GetProperty("movers").EnumerateArray().Select(m => m.GetProperty("criterion").GetString()).ToList();
        Assert.Equal(["Loan-to-value", "Debt service cover", "Sponsor track record"], names);
        Assert.Equal(15.0m, top.GetProperty("movers")[0].GetProperty("points").GetDecimal());
    }

    [Fact]
    public async Task AC3_given_a_change_to_a_weight_then_the_queue_reranks_without_a_deploy()
    {
        var (host, config) = await TriageHost.StartAsync();
        await using var _host = host;
        var token = await TriageHost.LoginAsync(host);
        var before = await TriageHost.GetJsonAsync(host, "/api/queue", token);

        // Head of credit decides only sponsor track record matters this week.
        config["Scoring:Weights:LoanToValue"] = "0";
        config["Scoring:Weights:DebtServiceCover"] = "0";
        config["Scoring:Weights:AssetType"] = "0";
        config["Scoring:Weights:Location"] = "0";
        config.Reload();

        var after = await TriageHost.GetJsonAsync(host, "/api/queue", token);
        Assert.False(before.GetProperty("criteriaVersion").GetString() == after.GetProperty("criteriaVersion").GetString(), "criteria version should change");
        Assert.Equal("Pier 7 Cold Store", TriageHost.DealNames(before)[0]);
        Assert.Equal("Rope Walk Student Housing", TriageHost.DealNames(after)[0]);
    }

    [Fact]
    public async Task An_invalid_weight_edit_is_rejected_and_the_last_good_weights_stay_live()
    {
        var (host, config) = await TriageHost.StartAsync();
        await using var _host = host;
        var token = await TriageHost.LoginAsync(host);
        var before = await TriageHost.GetJsonAsync(host, "/api/queue", token);

        config["Scoring:Weights:LoanToValue"] = "-5";
        config.Reload();

        var after = await TriageHost.GetJsonAsync(host, "/api/queue", token);
        Assert.Equal(before.GetProperty("criteriaVersion").GetString(), after.GetProperty("criteriaVersion").GetString());
        var criteria = await TriageHost.GetJsonAsync(host, "/api/criteria", token);
        Assert.Contains("LoanToValue", criteria.GetProperty("lastReloadError").GetString()!);
        Assert.Equal(5, criteria.GetProperty("criteria").GetArrayLength());
    }

    [Fact]
    public async Task The_queue_requires_a_signed_in_user()
    {
        var (host, _) = await TriageHost.StartAsync();
        await using var _host = host;

        using var response = await host.SendAsync(HttpMethod.Get, "/api/queue");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Take_outside_one_to_two_hundred_is_a_validation_problem()
    {
        var (host, _) = await TriageHost.StartAsync();
        await using var _host = host;
        var token = await TriageHost.LoginAsync(host);

        using var response = await host.SendAsync(HttpMethod.Get, "/api/queue?take=0", bearer: token);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("take", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Empty_queue_returns_no_items()
    {
        var (host, _) = await TriageHost.StartAsync(seed: false);
        await using var _host = host;
        var token = await TriageHost.LoginAsync(host);

        var queue = await TriageHost.GetJsonAsync(host, "/api/queue", token);

        Assert.Equal(0, queue.GetProperty("total").GetInt32());
        Assert.Equal(JsonValueKind.Array, queue.GetProperty("items").ValueKind);
    }
}
