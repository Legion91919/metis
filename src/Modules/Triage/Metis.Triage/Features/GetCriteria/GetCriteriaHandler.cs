using Agentic.Blocks.Core;
using Metis.Triage.Scoring;

namespace Metis.Triage.Features.GetCriteria;

internal sealed class GetCriteriaHandler(ScoringPolicyProvider policies)
{
    public Result<CriteriaResponse> Handle(GetCriteriaRequest request)
    {
        if (GetCriteriaValidator.Validate(request) is { } invalid) return invalid;
        var policy = policies.Current;
        var criteria = policy.Weighted
            .Select(w => new CriterionWeight(w.Criterion.Key, w.Criterion.Name, w.Weight, Math.Round(w.Weight / policy.TotalWeight, 4)))
            .ToList();
        return new CriteriaResponse(policy.Version, criteria, policies.LastReloadError);
    }
}
