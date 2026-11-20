using Agentic.Blocks.Core;

namespace Metis.Intake.Features.GetApplication;

internal static class GetApplicationValidator
{
    public static Error? Validate(GetApplicationRequest request) =>
        request.Id == Guid.Empty ? Error.Validation("intake.invalid_id", "id", "Id must not be empty.") : null;
}
