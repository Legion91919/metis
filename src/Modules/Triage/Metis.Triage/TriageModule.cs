using Metis.Triage.Explanation;
using Metis.Triage.Features.GetCriteria;
using Metis.Triage.Features.GetQueue;
using Metis.Triage.Scoring;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Metis.Triage;

/// <summary>Triage module: scores and ranks open applications with weights from configuration and explains each rank.</summary>
public static class TriageModule
{
    /// <summary>
    /// Registers the module. Binds "Scoring" with reload support; invalid criteria fail start-up and invalid edits are refused. Register your own
    /// <see cref="IDealExplainer"/> before this call to replace the template adapter.
    /// </summary>
    public static IServiceCollection AddTriageModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ScoringOptions>().Bind(configuration.GetSection(ScoringOptions.Section));
        services.AddSingleton<ScoringOptionsValidator>();
        services.AddSingleton<ScoringPolicyProvider>();
        services.AddHostedService<ScoringPolicyStartupCheck>();
        services.TryAddSingleton<IDealExplainer, TemplateDealExplainer>();
        services.AddSingleton<GuardedDealExplainer>();
        services.AddSingleton<GetQueueHandler>();
        services.AddSingleton<GetCriteriaHandler>();
        return services;
    }

    /// <summary>Maps the module's feature endpoints onto <paramref name="api"/>.</summary>
    public static IEndpointRouteBuilder MapTriageEndpoints(this IEndpointRouteBuilder api)
    {
        GetQueueEndpoint.Map(api);
        GetCriteriaEndpoint.Map(api);
        return api;
    }
}
