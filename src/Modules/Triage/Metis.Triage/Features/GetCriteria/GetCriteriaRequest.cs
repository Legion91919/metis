namespace Metis.Triage.Features.GetCriteria;

/// <summary>GET /api/criteria (no parameters).</summary>
internal sealed record GetCriteriaRequest;

internal sealed record CriteriaResponse(string Version, IReadOnlyList<CriterionWeight> Criteria, string? LastReloadError);

/// <summary>A criterion, its configured weight and its share of the total (0.30 = 30%).</summary>
internal sealed record CriterionWeight(string Key, string Name, decimal Weight, decimal Share);
