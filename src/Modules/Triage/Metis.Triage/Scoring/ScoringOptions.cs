using System.Globalization;
using Microsoft.Extensions.Options;

namespace Metis.Triage.Scoring;

/// <summary>Lending criteria owned by the head of credit. Bound from the "Scoring" section of criteria.json.</summary>
internal sealed class ScoringOptions
{
    public const string Section = "Scoring";

    /// <summary>Weight per criterion key (see <see cref="Criteria.Keys"/>). Relative: 30/20/25/15/10 = 30%/20%/...</summary>
    public Dictionary<string, decimal> Weights { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>LTV at or below Best scores 1; at or above Worst scores 0.</summary>
    public LinearBand LoanToValue { get; set; } = new();

    /// <summary>DSCR at or above Best scores 1; at or below Worst scores 0.</summary>
    public LinearBand DebtServiceCover { get; set; } = new();

    /// <summary>Completed deals for full marks on sponsor track record.</summary>
    public int SponsorDealsForFullMarks { get; set; }

    /// <summary>Score 0..1 per asset type. Unknown types score 0.</summary>
    public Dictionary<string, decimal> AssetTypeScores { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Score 0..1 per region. Unknown regions score 0.</summary>
    public Dictionary<string, decimal> LocationScores { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

internal sealed class LinearBand
{
    public decimal Best { get; set; }
    public decimal Worst { get; set; }
}

/// <summary>Rejects a bad edit: typos in keys, negative weights, all-zero weights, scores outside 0..1.</summary>
internal sealed class ScoringOptionsValidator : IValidateOptions<ScoringOptions>
{
    public ValidateOptionsResult Validate(string? name, ScoringOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var errors = new List<string>();
        foreach (var key in Criteria.Keys.Where(k => !options.Weights.ContainsKey(k)))
            errors.Add($"Weights:{key} is missing.");
        foreach (var (key, weight) in options.Weights)
        {
            if (!Criteria.Keys.Contains(key, StringComparer.OrdinalIgnoreCase)) errors.Add($"Weights:{key} is not a known criterion ({string.Join(", ", Criteria.Keys)}).");
            if (weight is < 0m or > 1000m) errors.Add($"Weights:{key} must be between 0 and 1000 (was {weight.ToString(CultureInfo.InvariantCulture)}).");
        }

        if (options.Weights.Values.Sum() <= 0m) errors.Add("At least one weight must be above zero.");
        if (options.LoanToValue.Best >= options.LoanToValue.Worst) errors.Add("LoanToValue:Best must be below LoanToValue:Worst.");
        if (options.DebtServiceCover.Best <= options.DebtServiceCover.Worst) errors.Add("DebtServiceCover:Best must be above DebtServiceCover:Worst.");
        if (options.SponsorDealsForFullMarks < 1) errors.Add("SponsorDealsForFullMarks must be at least 1.");
        foreach (var (key, score) in options.AssetTypeScores.Concat(options.LocationScores))
        {
            if (score is < 0m or > 1m) errors.Add($"Score for '{key}' must be between 0 and 1.");
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
