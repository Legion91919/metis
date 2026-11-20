using Agentic.Blocks.Core;
using Metis.Intake.Data;
using Metis.Intake.Shared;

namespace Metis.Intake.Features.GetApplication;

internal sealed class GetApplicationHandler(ApplicationStore store)
{
    public Result<ApplicationResponse> Handle(GetApplicationRequest request)
    {
        if (GetApplicationValidator.Validate(request) is { } invalid) return invalid;
        return store.Find(request.Id) is { } found
            ? ApplicationResponse.From(found)
            : Error.NotFound("intake.not_found", "No application with this id.");
    }
}
