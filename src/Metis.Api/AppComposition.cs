using Agentic.Blocks.Auth;
using Agentic.Blocks.Web;
using Metis.Intake;
using Metis.Triage;

namespace Metis.Api;

/// <summary>Composition root shared by Program and the acceptance tests.</summary>
public static class AppComposition
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var isDev = builder.Environment.IsDevelopment();
        builder.Services.TryAddTimeProvider();
        builder.Services.AddBlocksWeb().AddBlocksAuth(o =>
        {
            // Development only: invented demo users. Password comes from DemoUsers__Password (see .env.example);
            // when it is not set, the Auth block generates one and logs it once.
            var demoSecret = builder.Configuration["DemoUsers:Password"];
            var analyst = new DemoUser { Email = "analyst@example.com", Password = demoSecret }; // privacy-guard: ignore (variable from DemoUsers__Password, not a credential)
            analyst.Roles.Add(IntakeModule.SubmitRole);
            var credit = new DemoUser { Email = "credit@example.com", Password = demoSecret }; // privacy-guard: ignore
            credit.Roles.Add("credit");
            credit.Roles.Add(IntakeModule.SubmitRole);
            o.DemoUsers.Add(analyst);
            o.DemoUsers.Add(credit);
        });
        builder.Services.AddIntakeModule(seedDemoData: builder.Configuration.GetValue("Intake:SeedDemoData", isDev));
        builder.Services.AddTriageModule(builder.Configuration);
    }

    public static void Configure(WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.UseBlocksWeb();
        app.MapBlocksAuth("/auth");
        var api = app.MapGroup("/api").RequireAuthorization();
        api.MapIntakeEndpoints();
        api.MapTriageEndpoints();
    }

    private static void TryAddTimeProvider(this IServiceCollection services)
    {
        if (!services.Any(d => d.ServiceType == typeof(TimeProvider))) services.AddSingleton(TimeProvider.System);
    }
}
