using Agentic.Blocks.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Metis.Triage.Features.GetCriteria;

internal static class GetCriteriaEndpoint
{
    public static void Map(IEndpointRouteBuilder api) =>
        api.MapGet("/criteria", (GetCriteriaHandler handler) => handler.Handle(new GetCriteriaRequest()).ToHttpResult())
            .WithName("GetCriteria");
}
