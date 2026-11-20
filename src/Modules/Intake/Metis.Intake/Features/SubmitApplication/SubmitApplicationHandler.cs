using Agentic.Blocks.Core;
using Metis.Intake.Contracts;
using Metis.Intake.Data;
using Metis.Intake.Shared;

namespace Metis.Intake.Features.SubmitApplication;

/// <summary>Outcome: the stored application and whether this call created it (false = replay of an earlier call).</summary>
internal sealed record SubmitApplicationResult(ApplicationResponse Application, bool Created);

internal sealed class SubmitApplicationHandler(ApplicationStore store, TimeProvider clock)
{
    public Result<SubmitApplicationResult> Handle(SubmitApplicationRequest request)
    {
        if (SubmitApplicationValidator.Validate(request) is { } invalid) return invalid;

        var candidate = new ApplicationSnapshot(
            request.Id, request.DealName!.Trim(), request.Sponsor!.Trim(), request.AssetType!, request.Region!.Trim(),
            Money.Of(request.LoanAmount, request.Currency!), Money.Of(request.PropertyValue, request.Currency!),
            request.DebtServiceCover, request.SponsorCompletedDeals, clock.GetUtcNow());

        if (store.TryAdd(candidate)) return new SubmitApplicationResult(ApplicationResponse.From(candidate), Created: true);

        var existing = store.Find(request.Id)!;
        return SameContent(existing, candidate)
            ? new SubmitApplicationResult(ApplicationResponse.From(existing), Created: false)
            : Error.Conflict("intake.id_conflict", "An application with this id already exists with different content.");
    }

    private static bool SameContent(ApplicationSnapshot a, ApplicationSnapshot b) => a with { ReceivedAt = b.ReceivedAt } == b;
}
