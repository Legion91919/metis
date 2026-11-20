using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Metis.Triage.Scoring;

/// <summary>An immutable snapshot of the criteria with a short version hash, so every rank can say which weights produced it.</summary>
internal sealed class ScoringPolicy
{
    private ScoringPolicy(ScoringOptions options, IReadOnlyList<(Criterion Criterion, decimal Weight)> weighted, string version)
    {
        Options = options;
        Weighted = weighted;
        Version = version;
        TotalWeight = weighted.Sum(w => w.Weight);
    }

    public ScoringOptions Options { get; }

    /// <summary>Criteria in canonical order with their weights.</summary>
    public IReadOnlyList<(Criterion Criterion, decimal Weight)> Weighted { get; }

    public decimal TotalWeight { get; }

    /// <summary>First 8 hex characters of SHA-256 over a canonical form of the options.</summary>
    public string Version { get; }

    public static ScoringPolicy From(ScoringOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var weighted = Criteria.All.Select(c => (c, options.Weights.TryGetValue(c.Key, out var w) ? w : 0m)).ToList();
        return new ScoringPolicy(options, weighted, Hash(options));
    }

    private static string Hash(ScoringOptions o)
    {
        var inv = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        void Table(string name, Dictionary<string, decimal> table)
        {
            foreach (var (k, v) in table.OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase))
                sb.Append(name).Append(':').Append(k.ToUpperInvariant()).Append('=').Append(v.ToString("0.####", inv)).Append(';');
        }

        Table("w", o.Weights);
        Table("a", o.AssetTypeScores);
        Table("l", o.LocationScores);
        sb.Append(inv, $"ltv={o.LoanToValue.Best:0.####}/{o.LoanToValue.Worst:0.####};dscr={o.DebtServiceCover.Best:0.####}/{o.DebtServiceCover.Worst:0.####};s={o.SponsorDealsForFullMarks}");
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(hash, 0, 4).ToLowerInvariant();
    }
}
