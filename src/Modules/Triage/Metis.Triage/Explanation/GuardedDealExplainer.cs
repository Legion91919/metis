using Microsoft.Extensions.Logging;

namespace Metis.Triage.Explanation;

/// <summary>Rules any explainer output must follow before a user sees it.</summary>
internal static class ExplanationGuard
{
    public const int MaxLength = 800;

    public static bool IsAcceptable(string? text, ExplanationFacts facts) =>
        !string.IsNullOrWhiteSpace(text)
        && text.Length <= MaxLength
        && !text.Contains('\n', StringComparison.Ordinal)
        && facts.TopMovers.All(m => text.Contains(m.Criterion, StringComparison.OrdinalIgnoreCase));
}

/// <summary>
/// Wraps the configured <see cref="IDealExplainer"/> with a timeout and the guard. On failure, timeout or a rule
/// break it falls back to the template, so the queue never waits on, or shows unchecked text from, a model.
/// </summary>
internal sealed partial class GuardedDealExplainer(IDealExplainer inner, TimeProvider clock, ILogger<GuardedDealExplainer> logger)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);
    private readonly TemplateDealExplainer _fallback = new();

    public async Task<string> ExplainAsync(ExplanationFacts facts, CancellationToken cancellationToken)
    {
        if (inner is TemplateDealExplainer) return await _fallback.ExplainAsync(facts, cancellationToken).ConfigureAwait(false);
        using var timeout = new CancellationTokenSource(Timeout, clock);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        try
        {
            var text = await inner.ExplainAsync(facts, linked.Token).ConfigureAwait(false);
            if (ExplanationGuard.IsAcceptable(text, facts)) return text.Trim();
            LogRejected(logger, inner.GetType().Name);
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or InvalidOperationException && !cancellationToken.IsCancellationRequested)
        {
            LogFailed(logger, inner.GetType().Name, ex.GetType().Name);
        }

        return await _fallback.ExplainAsync(facts, cancellationToken).ConfigureAwait(false);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Explainer {Adapter} output broke the guard; using the template")]
    private static partial void LogRejected(ILogger logger, string adapter);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Explainer {Adapter} failed ({Error}); using the template")]
    private static partial void LogFailed(ILogger logger, string adapter, string error);
}
