using Metis.Intake.Contracts;
using Metis.Intake.Data;
using Metis.Intake.Features.GetApplication;
using Metis.Intake.Features.SubmitApplication;
using Metis.Intake.Seeding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Metis.Intake;

/// <summary>Intake module: receives loan applications and owns their data.</summary>
public static class IntakeModule
{
    /// <summary>Role allowed to submit applications.</summary>
    public const string SubmitRole = "origination";

    /// <summary>Registers the module. <paramref name="seedDemoData"/> adds invented applications on start.</summary>
    public static IServiceCollection AddIntakeModule(this IServiceCollection services, bool seedDemoData)
    {
        services.AddSingleton<ApplicationStore>();
        services.AddSingleton<IApplicationReader>(sp => sp.GetRequiredService<ApplicationStore>());
        services.AddSingleton<SubmitApplicationHandler>();
        services.AddSingleton<GetApplicationHandler>();
        if (seedDemoData) services.AddHostedService<DemoApplications>();
        return services;
    }

    /// <summary>Maps the module's feature endpoints onto <paramref name="api"/>.</summary>
    public static IEndpointRouteBuilder MapIntakeEndpoints(this IEndpointRouteBuilder api)
    {
        SubmitApplicationEndpoint.Map(api);
        GetApplicationEndpoint.Map(api);
        return api;
    }
}
