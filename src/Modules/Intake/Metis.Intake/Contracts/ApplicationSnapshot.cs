using Agentic.Blocks.Core;

namespace Metis.Intake.Contracts;

/// <summary>
/// The public, read-only view of a loan application that other modules may use.
/// Invented example data only; no personal data is held (sponsors are companies).
/// </summary>
public sealed record ApplicationSnapshot(
    Guid Id,
    string DealName,
    string Sponsor,
    string AssetType,
    string Region,
    Money LoanAmount,
    Money PropertyValue,
    decimal DebtServiceCover,
    int SponsorCompletedDeals,
    DateTimeOffset ReceivedAt)
{
    /// <summary>Loan amount divided by property value (0.538 = 53.8%).</summary>
    public decimal LoanToValue => PropertyValue.Amount == 0m ? 1m : LoanAmount.Amount / PropertyValue.Amount;
}

/// <summary>Read port Intake offers to other modules.</summary>
public interface IApplicationReader
{
    /// <summary>All applications still waiting for review.</summary>
    Task<IReadOnlyList<ApplicationSnapshot>> ListOpenAsync(CancellationToken cancellationToken);
}

/// <summary>The asset type vocabulary Intake accepts.</summary>
public static class AssetTypes
{
    /// <summary>Accepted values, case-sensitive.</summary>
    public static readonly IReadOnlyList<string> All = ["Industrial", "Multifamily", "MixedUse", "Retail", "Office", "Hotel", "Land"];
}
