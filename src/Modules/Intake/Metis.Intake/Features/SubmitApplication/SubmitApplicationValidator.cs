using Agentic.Blocks.Core;
using Metis.Intake.Contracts;

namespace Metis.Intake.Features.SubmitApplication;

internal static class SubmitApplicationValidator
{
    public static Error? Validate(SubmitApplicationRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.Id == Guid.Empty) errors["id"] = ["A client-generated id is required so retries are safe."];
        Text(errors, "dealName", request.DealName, 120);
        Text(errors, "sponsor", request.Sponsor, 120);
        Text(errors, "region", request.Region, 60);
        if (request.AssetType is null || !AssetTypes.All.Contains(request.AssetType))
            errors["assetType"] = [$"Must be one of: {string.Join(", ", AssetTypes.All)}."];
        if (request.LoanAmount <= 0m) errors["loanAmount"] = ["Must be greater than zero."];
        if (request.PropertyValue <= 0m) errors["propertyValue"] = ["Must be greater than zero."];
        if (Money.TryOf(1m, request.Currency).IsFailure) errors["currency"] = ["Must be an ISO 4217 code such as GBP."];
        if (request.DebtServiceCover is < 0m or > 20m) errors["debtServiceCover"] = ["Must be between 0 and 20."];
        if (request.SponsorCompletedDeals is < 0 or > 1000) errors["sponsorCompletedDeals"] = ["Must be between 0 and 1000."];
        return errors.Count == 0 ? null : Error.Validation("intake.invalid_application", "The application is not valid.", errors);
    }

    private static void Text(Dictionary<string, string[]> errors, string field, string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > max) errors[field] = [$"Required, at most {max} characters."];
    }
}
