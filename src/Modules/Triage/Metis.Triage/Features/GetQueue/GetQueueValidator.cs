using Agentic.Blocks.Core;

namespace Metis.Triage.Features.GetQueue;

internal static class GetQueueValidator
{
    public const int MaxTake = 200;

    public static Error? Validate(GetQueueRequest request) =>
        request.Take is < 1 or > MaxTake
            ? Error.Validation("triage.invalid_take", "take", $"take must be between 1 and {MaxTake}.")
            : null;
}
