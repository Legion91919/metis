using Agentic.Blocks.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Metis.Triage.Features.GetQueue;

internal static class GetQueueEndpoint
{
    public static void Map(IEndpointRouteBuilder api) =>
        api.MapGet("/queue", async (int? take, GetQueueHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(new GetQueueRequest(take ?? GetQueueRequest.DefaultTake), ct).ConfigureAwait(false)).ToHttpResult())
            .WithName("GetQueue");
}
