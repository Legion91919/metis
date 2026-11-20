namespace Metis.Triage.Explanation;

/// <summary>
/// Port: turns already-decided facts into one readable paragraph. Implementations may use a language model, but they
/// only ever see these facts and can never change a score, a rank or the movers. See docs/DESIGN.md (LLM adapter).
/// </summary>
public interface IDealExplainer
{
    /// <summary>Returns one paragraph (no line breaks) that names every top mover.</summary>
    Task<string> ExplainAsync(ExplanationFacts facts, CancellationToken cancellationToken);
}

/// <summary>Everything an explainer may use. No personal data: deal and criterion names, numbers and the criteria version.</summary>
public sealed record ExplanationFacts(
    string DealName,
    int Rank,
    int QueueSize,
    decimal Score,
    IReadOnlyList<ScoreMover> TopMovers,
    decimal OtherPoints,
    string CriteriaVersion);

/// <summary>A criterion that moved the score, its evidence ("53.8%") and signed points.</summary>
public sealed record ScoreMover(string Criterion, string Evidence, decimal Points);
