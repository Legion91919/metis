using System.Globalization;

namespace Metis.Triage.Explanation;

/// <summary>Default adapter: a fixed sentence template. Deterministic, instant, free, and always passes the guard.</summary>
internal sealed class TemplateDealExplainer : IDealExplainer
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public Task<string> ExplainAsync(ExplanationFacts facts, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(facts);
        var movers = facts.TopMovers.Select(m => $"{m.Criterion.ToLowerInvariant()} of {m.Evidence} ({Signed(m.Points)} points)").ToList();
        var list = movers.Count switch
        {
            0 => "no single criterion",
            1 => movers[0],
            _ => string.Join(", ", movers.Take(movers.Count - 1)) + " and " + movers[^1],
        };
        var text =
            $"{facts.DealName} ranks #{facts.Rank.ToString(Inv)} of {facts.QueueSize.ToString(Inv)} with a score of {facts.Score.ToString("0.0", Inv)} out of 100. " +
            $"The criteria that moved it most were {list}; the remaining criteria moved it {Signed(facts.OtherPoints)} points in total. " +
            $"Scored with criteria version {facts.CriteriaVersion}; the score comes only from the weighted lending criteria.";
        return Task.FromResult(text);
    }

    private static string Signed(decimal points) => Math.Round(points, 1, MidpointRounding.ToEven).ToString("+0.0;-0.0;0.0", Inv);
}
