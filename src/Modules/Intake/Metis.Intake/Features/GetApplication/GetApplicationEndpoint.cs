using Agentic.Blocks.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Metis.Intake.Features.GetApplication;

internal static class GetApplicationEndpoint
{
    public static void Map(IEndpointRouteBuilder api) =>
        api.MapGet("/applications/{id:guid}", (Guid id, GetApplicationHandler handler) => handler.Handle(new GetApplicationRequest(id)).ToHttpResult())
            .WithName("GetApplication");
}
