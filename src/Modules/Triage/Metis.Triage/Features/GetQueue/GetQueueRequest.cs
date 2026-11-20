namespace Metis.Triage.Features.GetQueue;

/// <summary>GET /api/queue?take=50</summary>
internal sealed record GetQueueRequest(int Take = GetQueueRequest.DefaultTake)
{
    public const int DefaultTake = 50;
}

internal sealed record QueueResponse(string CriteriaVersion, DateTimeOffset AsOf, int Total, IReadOnlyList<QueueItem> Items);

internal sealed record QueueItem(
    int Rank,
    Guid ApplicationId,
    string DealName,
    string Sponsor,
    string AssetType,
    string Region,
    decimal LoanAmount,
    string Currency,
    decimal LoanToValue,
    decimal DebtServiceCover,
    decimal Score,
    IReadOnlyList<MoverDto> Movers,
    string Explanation,
    int WaitingHours,
    int ArrivalPosition);

internal sealed record MoverDto(string Key, string Criterion, string Evidence, decimal Points);
