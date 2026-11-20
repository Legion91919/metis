using Agentic.Blocks.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Metis.Intake.Features.SubmitApplication;

internal static class SubmitApplicationEndpoint
{
    public static void Map(IEndpointRouteBuilder api) =>
        api.MapPost("/applications", (SubmitApplicationRequest request, SubmitApplicationHandler handler) =>
                handler.Handle(request).ToHttpResult(r => r.Created
                    ? Results.Created($"/api/applications/{r.Application.Id}", r.Application)
                    : Results.Ok(r.Application)))
            .WithName("SubmitApplication")
            .RequireAuthorization(p => p.RequireRole(IntakeModule.SubmitRole));
}
