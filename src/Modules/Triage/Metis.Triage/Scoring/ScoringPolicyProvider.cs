using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Metis.Triage.Scoring;

/// <summary>
/// Hands out the current policy. Reads IOptionsMonitor on every call, so an edit to criteria.json takes effect on the
/// next request without a deploy. Validation happens here rather than in the options pipeline: an invalid edit is
/// refused, the last good policy stays live and the reason is reported, instead of an exception in the reload callback.
/// </summary>
internal sealed partial class ScoringPolicyProvider(IOptionsMonitor<ScoringOptions> monitor, ScoringOptionsValidator validator, ILogger<ScoringPolicyProvider> logger)
{
    private readonly object _gate = new();
    private ScoringOptions? _lastOptions;
    private ScoringPolicy? _lastPolicy;

    /// <summary>Why the latest edit was refused, or null when the live policy is the latest one.</summary>
    public string? LastReloadError { get; private set; }

    public ScoringPolicy Current
    {
        get
        {
            lock (_gate)
            {
                var options = monitor.CurrentValue;
                if (ReferenceEquals(options, _lastOptions)) return _lastPolicy!;

                var check = validator.Validate(null, options);
                if (check.Failed)
                {
                    if (_lastPolicy is null) throw new OptionsValidationException(ScoringOptions.Section, typeof(ScoringOptions), check.Failures);
                    if (LastReloadError != check.FailureMessage) LogEditRefused(logger, _lastPolicy.Version, check.FailureMessage);
                    LastReloadError = check.FailureMessage;
                    return _lastPolicy;
                }

                _lastPolicy = ScoringPolicy.From(options);
                _lastOptions = options;
                LastReloadError = null;
                LogPolicyLoaded(logger, _lastPolicy.Version);
                return _lastPolicy;
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Scoring criteria {Version} are live")]
    private static partial void LogPolicyLoaded(ILogger logger, string version);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Criteria edit refused, keeping {Version}: {Reason}")]
    private static partial void LogEditRefused(ILogger logger, string version, string reason);
}

/// <summary>Fails start-up when the criteria are invalid, so a bad first deploy never serves a queue.</summary>
internal sealed class ScoringPolicyStartupCheck(ScoringPolicyProvider provider) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = provider.Current;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
