using Metis.Intake.Contracts;

namespace Metis.Triage.Scoring;

/// <summary>How one criterion moved a score. <see cref="Points"/> is signed: above 0 pushed the deal up.</summary>
internal sealed record CriterionScore(string Key, string Name, decimal Weight, decimal SubScore, decimal Points, string Evidence);

internal sealed record DealScore(ApplicationSnapshot Application, decimal Score, IReadOnlyList<CriterionScore> Criteria, IReadOnlyList<CriterionScore> TopMovers);

/// <summary>
/// Pure, deterministic scoring. Every deal starts at 50 (neutral). Each criterion adds
/// weight / total weight x (sub-score - 0.5) x 100 points, so the score is the weighted average of sub-scores on 0..100
/// and each criterion's points say exactly how far it moved the deal. All arithmetic is decimal.
/// </summary>
internal static class ScoringEngine
{
    public const decimal Neutral = 50m;

    public static DealScore Score(ApplicationSnapshot application, ScoringPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(policy);
        var criteria = new List<CriterionScore>(policy.Weighted.Count);
        foreach (var (criterion, weight) in policy.Weighted)
        {
            var (sub, evidence) = criterion.Evaluate(application, policy);
            var points = weight / policy.TotalWeight * (sub - 0.5m) * 100m;
            criteria.Add(new CriterionScore(criterion.Key, criterion.Name, weight, sub, points, evidence));
        }

        var score = Math.Round(Neutral + criteria.Sum(c => c.Points), 1, MidpointRounding.ToEven);
        var movers = criteria
            .Select((c, index) => (c, index))
            .Where(x => x.c.Weight > 0m)
            .OrderByDescending(x => Math.Abs(x.c.Points))
            .ThenBy(x => x.index)
            .Take(3)
            .Select(x => x.c)
            .ToList();
        return new DealScore(application, score, criteria, movers);
    }
}

internal sealed record RankedDeal(int Rank, int ArrivalPosition, DealScore Score);

/// <summary>Orders deals by score (high first), then arrival (oldest first), then id, so the order is total and repeatable.</summary>
internal static class QueueRanker
{
    public static IReadOnlyList<RankedDeal> Rank(IEnumerable<ApplicationSnapshot> applications, ScoringPolicy policy)
    {
        var list = applications.ToList();
        var arrival = list.OrderBy(a => a.ReceivedAt).ThenBy(a => a.Id)
            .Select((a, i) => (a.Id, Position: i + 1)).ToDictionary(x => x.Id, x => x.Position);
        return list
            .Select(a => ScoringEngine.Score(a, policy))
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.Application.ReceivedAt)
            .ThenBy(s => s.Application.Id)
            .Select((s, i) => new RankedDeal(i + 1, arrival[s.Application.Id], s))
            .ToList();
    }
}
