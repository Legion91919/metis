using Agentic.Blocks.Core;

namespace Metis.Triage.Features.GetCriteria;

/// <summary>Nothing to check today; kept so every slice has the same four parts and a place for future filters.</summary>
internal static class GetCriteriaValidator
{
    public static Error? Validate(GetCriteriaRequest request) => request is null ? Error.Validation("triage.invalid_request", "Request is required.") : null;
}
