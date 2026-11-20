using Agentic.Blocks.Core;
using Metis.Intake.Contracts;
using Metis.Triage.Explanation;
using Metis.Triage.Scoring;

namespace Metis.Triage.Features.GetQueue;

/// <summary>Scores every open application with the live criteria, ranks them and explains each rank.</summary>
internal sealed class GetQueueHandler(IApplicationReader applications, ScoringPolicyProvider policies, GuardedDealExplainer explainer, TimeProvider clock)
{
    public async Task<Result<QueueResponse>> HandleAsync(GetQueueRequest request, CancellationToken cancellationToken)
    {
        if (GetQueueValidator.Validate(request) is { } invalid) return invalid;

        var policy = policies.Current;
        var open = await applications.ListOpenAsync(cancellationToken).ConfigureAwait(false);
        var ranked = QueueRanker.Rank(open, policy);
        var now = clock.GetUtcNow();
        var items = new List<QueueItem>();
        foreach (var deal in ranked.Take(request.Take))
        {
            var s = deal.Score;
            var a = s.Application;
            var movers = s.TopMovers.Select(m => new MoverDto(m.Key, m.Name, m.Evidence, Round(m.Points))).ToList();
            var facts = new ExplanationFacts(a.DealName, deal.Rank, ranked.Count, s.Score,
                [.. movers.Select(m => new ScoreMover(m.Criterion, m.Evidence, m.Points))],
                s.Criteria.Except(s.TopMovers).Sum(c => c.Points), policy.Version);
            var explanation = await explainer.ExplainAsync(facts, cancellationToken).ConfigureAwait(false);
            items.Add(new QueueItem(deal.Rank, a.Id, a.DealName, a.Sponsor, a.AssetType, a.Region, a.LoanAmount.Amount, a.LoanAmount.Currency,
                Math.Round(a.LoanToValue, 4), a.DebtServiceCover, s.Score, movers, explanation,
                (int)Math.Max(0, (now - a.ReceivedAt).TotalHours), deal.ArrivalPosition));
        }

        return new QueueResponse(policy.Version, now, ranked.Count, items);
    }

    private static decimal Round(decimal points) => Math.Round(points, 1, MidpointRounding.ToEven);
}
