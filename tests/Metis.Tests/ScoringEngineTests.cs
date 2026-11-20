using Agentic.Blocks.Core;
using Metis.Intake.Contracts;
using Metis.Triage.Scoring;

namespace Metis.Tests;

public class ScoringEngineTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 9, 0, 0, TimeSpan.Zero);

    internal static ScoringOptions DefaultOptions() => new()
    {
        Weights = new(StringComparer.OrdinalIgnoreCase)
        {
            ["LoanToValue"] = 30m,
            ["DebtServiceCover"] = 20m,
            ["SponsorTrackRecord"] = 25m,
            ["AssetType"] = 15m,
            ["Location"] = 10m,
        },
        LoanToValue = new LinearBand { Best = 0.55m, Worst = 0.80m },
        DebtServiceCover = new LinearBand { Best = 1.6m, Worst = 1.0m },
        SponsorDealsForFullMarks = 12,
        AssetTypeScores = new(StringComparer.OrdinalIgnoreCase) { ["Industrial"] = 1.0m, ["Multifamily"] = 0.85m, ["Office"] = 0.35m, ["Hotel"] = 0.3m },
        LocationScores = new(StringComparer.OrdinalIgnoreCase) { ["Harbourside"] = 1.0m, ["Northgate"] = 0.6m, ["Millbrook"] = 0.7m },
    };

    internal static ApplicationSnapshot Deal(string name, string asset, string region, decimal loan, decimal value, decimal dscr, int deals, int hoursAgo, Guid? id = null) =>
        new(id ?? Guid.NewGuid(), name, "Invented Sponsor", asset, region, Money.Of(loan, "GBP"), Money.Of(value, "GBP"), dscr, deals, Now.AddHours(-hoursAgo));

    private static readonly ApplicationSnapshot Pier7 = Deal("Pier 7 Cold Store", "Industrial", "Harbourside", 4_200_000m, 7_800_000m, 1.62m, 10, 30);
    private static readonly ApplicationSnapshot RopeWalk = Deal("Rope Walk Student Housing", "Multifamily", "Northgate", 7_750_000m, 12_400_000m, 1.55m, 11, 10);
    private static readonly ApplicationSnapshot Tannery = Deal("Tannery Lane Offices", "Office", "Northgate", 3_100_000m, 4_200_000m, 1.25m, 2, 40);
    private static readonly ApplicationSnapshot Linden = Deal("Linden Row Apartments", "Multifamily", "Millbrook", 9_500_000m, 13_200_000m, 1.31m, 6, 52);

    [Fact]
    public void Score_is_fifty_plus_the_weighted_contributions_of_each_criterion()
    {
        var score = ScoringEngine.Score(Pier7, ScoringPolicy.From(DefaultOptions()));

        Assert.Equal(95.8m, score.Score);
        Assert.Equal(5, score.Criteria.Count);
        Assert.Equal(["LoanToValue", "DebtServiceCover", "SponsorTrackRecord", "AssetType", "Location"], score.Criteria.Select(c => c.Key).ToList());
    }

    [Fact]
    public void Same_weights_and_same_deals_give_the_same_order_whatever_the_input_order()
    {
        var policy = ScoringPolicy.From(DefaultOptions());
        ApplicationSnapshot[] deals = [Tannery, Pier7, Linden, RopeWalk];

        var a = QueueRanker.Rank(deals, policy).Select(r => r.Score.Application.DealName).ToList();
        var b = QueueRanker.Rank(deals.Reverse(), policy).Select(r => r.Score.Application.DealName).ToList();

        Assert.Equal(a, b);
        Assert.Equal(["Pier 7 Cold Store", "Rope Walk Student Housing", "Linden Row Apartments", "Tannery Lane Offices"], a);
    }

    [Fact]
    public void Equal_scores_are_ordered_by_arrival_then_by_id()
    {
        var policy = ScoringPolicy.From(DefaultOptions());
        var older = Deal("Older twin", "Industrial", "Harbourside", 1m, 2m, 1.6m, 12, 5, Guid.Parse("00000000-0000-0000-0000-000000000009"));
        var newer = Deal("Newer twin", "Industrial", "Harbourside", 1m, 2m, 1.6m, 12, 1, Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var sameTimeLowId = older with { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), DealName = "Same time, lower id" };

        var ranked = QueueRanker.Rank([newer, older, sameTimeLowId], policy);

        Assert.Equal(["Same time, lower id", "Older twin", "Newer twin"], ranked.Select(r => r.Score.Application.DealName).ToList());
        Assert.Equal([1, 2, 3], ranked.Select(r => r.ArrivalPosition).ToList());
    }

    [Fact]
    public void Top_movers_are_the_three_largest_absolute_contributions()
    {
        var score = ScoringEngine.Score(Tannery, ScoringPolicy.From(DefaultOptions()));

        Assert.Equal(["SponsorTrackRecord", "LoanToValue", "AssetType"], score.TopMovers.Select(m => m.Key).ToList());
        Assert.True(score.TopMovers[0].Points < 0);
        Assert.Equal("2 completed deals", score.TopMovers[0].Evidence);
    }

    [Fact]
    public void Equal_contributions_keep_the_canonical_criterion_order()
    {
        // LTV and DSCR both at full marks with equal weights contribute the same points.
        var options = DefaultOptions();
        options.Weights["DebtServiceCover"] = 30m;
        var deal = Deal("Tie", "Office", "Northgate", 50m, 100m, 1.7m, 6, 1);

        var score = ScoringEngine.Score(deal, ScoringPolicy.From(options));

        Assert.Equal("LoanToValue", score.TopMovers[0].Key);
        Assert.Equal("DebtServiceCover", score.TopMovers[1].Key);
    }

    [Fact]
    public void A_criterion_with_zero_weight_never_appears_as_a_mover()
    {
        var options = DefaultOptions();
        options.Weights["LoanToValue"] = 0m;

        var score = ScoringEngine.Score(Pier7, ScoringPolicy.From(options));

        Assert.DoesNotContain(score.TopMovers, m => m.Key == "LoanToValue");
        Assert.Equal(3, score.TopMovers.Count);
    }

    [Theory]
    [InlineData(0.50, 1.0)]
    [InlineData(0.55, 1.0)]
    [InlineData(0.675, 0.5)]
    [InlineData(0.80, 0.0)]
    [InlineData(0.95, 0.0)]
    public void Loan_to_value_scores_linearly_between_best_and_worst(double ltv, double expected)
    {
        var deal = Deal("LTV", "Industrial", "Harbourside", (decimal)ltv * 1_000_000m, 1_000_000m, 1.6m, 12, 1);

        var score = ScoringEngine.Score(deal, ScoringPolicy.From(DefaultOptions()));

        Assert.Equal((decimal)expected, score.Criteria.Single(c => c.Key == "LoanToValue").SubScore);
    }

    [Fact]
    public void Unknown_asset_type_or_region_scores_zero_and_says_so()
    {
        var deal = Deal("Unknown", "Land", "Farfield", 1m, 2m, 1.6m, 12, 1);

        var score = ScoringEngine.Score(deal, ScoringPolicy.From(DefaultOptions()));

        var asset = score.Criteria.Single(c => c.Key == "AssetType");
        Assert.Equal(0m, asset.SubScore);
        Assert.Contains("not in criteria", asset.Evidence);
    }

    [Fact]
    public void Changing_a_weight_changes_the_version_and_the_same_options_keep_it()
    {
        var a = ScoringPolicy.From(DefaultOptions());
        var b = ScoringPolicy.From(DefaultOptions());
        var changed = DefaultOptions();
        changed.Weights["Location"] = 11m;

        Assert.Equal(a.Version, b.Version);
        Assert.False(a.Version == ScoringPolicy.From(changed).Version, "version should change");
        Assert.Equal(8, a.Version.Length);
    }

    [Theory]
    [InlineData("LoanToValue", "-1")]
    [InlineData("Typo", "10")]
    public void Invalid_weights_fail_validation(string key, string value)
    {
        var options = DefaultOptions();
        options.Weights[key] = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

        var result = new ScoringOptionsValidator().Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(key, result.FailureMessage);
    }

    [Fact]
    public void All_zero_weights_or_a_missing_criterion_fail_validation()
    {
        var zero = DefaultOptions();
        foreach (var key in zero.Weights.Keys.ToList()) zero.Weights[key] = 0m;
        var missing = DefaultOptions();
        missing.Weights.Remove("Location");

        Assert.True(new ScoringOptionsValidator().Validate(null, zero).Failed);
        Assert.Contains("Location", new ScoringOptionsValidator().Validate(null, missing).FailureMessage);
        Assert.False(new ScoringOptionsValidator().Validate(null, DefaultOptions()).Failed);
    }
}
