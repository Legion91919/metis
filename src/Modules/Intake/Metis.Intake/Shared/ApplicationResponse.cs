namespace Metis.Intake.Shared;

/// <summary>The application as stored, returned by submit and get.</summary>
internal sealed record ApplicationResponse(
    Guid Id,
    string DealName,
    string Sponsor,
    string AssetType,
    string Region,
    decimal LoanAmount,
    decimal PropertyValue,
    string Currency,
    decimal LoanToValue,
    decimal DebtServiceCover,
    int SponsorCompletedDeals,
    DateTimeOffset ReceivedAt)
{
    public static ApplicationResponse From(Contracts.ApplicationSnapshot a) => new(
        a.Id, a.DealName, a.Sponsor, a.AssetType, a.Region, a.LoanAmount.Amount, a.PropertyValue.Amount, a.LoanAmount.Currency,
        Math.Round(a.LoanToValue, 4), a.DebtServiceCover, a.SponsorCompletedDeals, a.ReceivedAt);
}
