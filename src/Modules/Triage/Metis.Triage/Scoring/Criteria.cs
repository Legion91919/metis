using System.Globalization;
using Metis.Intake.Contracts;

namespace Metis.Triage.Scoring;

/// <summary>One lending criterion: turns an application into a sub-score from 0 (worst) to 1 (best) plus evidence text.</summary>
internal sealed record Criterion(string Key, string Name, Func<ApplicationSnapshot, ScoringPolicy, (decimal SubScore, string Evidence)> Evaluate);

/// <summary>
/// The criteria in canonical order. The order is part of the contract: it breaks ties between equal contributions,
/// so the same weights always give the same reasons in the same order.
/// </summary>
internal static class Criteria
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static readonly IReadOnlyList<Criterion> All =
    [
        new("LoanToValue", "Loan-to-value", (a, p) =>
            (Linear(a.LoanToValue, p.Options.LoanToValue.Worst, p.Options.LoanToValue.Best), (a.LoanToValue * 100m).ToString("0.0", Inv) + "%")),
        new("DebtServiceCover", "Debt service cover", (a, p) =>
            (Linear(a.DebtServiceCover, p.Options.DebtServiceCover.Worst, p.Options.DebtServiceCover.Best), a.DebtServiceCover.ToString("0.00", Inv) + "x")),
        new("SponsorTrackRecord", "Sponsor track record", (a, p) =>
            (Clamp((decimal)a.SponsorCompletedDeals / p.Options.SponsorDealsForFullMarks), $"{a.SponsorCompletedDeals.ToString(Inv)} completed deal{(a.SponsorCompletedDeals == 1 ? "" : "s")}")),
        new("AssetType", "Asset type", (a, p) => Lookup(p.Options.AssetTypeScores, a.AssetType)),
        new("Location", "Location", (a, p) => Lookup(p.Options.LocationScores, a.Region)),
    ];

    public static readonly IReadOnlyList<string> Keys = [.. All.Select(c => c.Key)];

    /// <summary>0 at <paramref name="worst"/>, 1 at <paramref name="best"/>, straight line between (works for either direction).</summary>
    internal static decimal Linear(decimal value, decimal worst, decimal best) => Clamp((value - worst) / (best - worst));

    private static decimal Clamp(decimal value) => Math.Round(Math.Clamp(value, 0m, 1m), 4, MidpointRounding.ToEven);

    private static (decimal, string) Lookup(Dictionary<string, decimal> table, string value) =>
        table.TryGetValue(value, out var score) ? (Clamp(score), value) : (0m, $"{value} (not in criteria)");
}
