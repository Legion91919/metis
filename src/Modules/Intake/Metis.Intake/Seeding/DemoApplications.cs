using Agentic.Blocks.Core;
using Metis.Intake.Contracts;
using Metis.Intake.Data;
using Microsoft.Extensions.Hosting;

namespace Metis.Intake.Seeding;

/// <summary>Seeds invented applications (Development only). All names and numbers are made up.</summary>
internal sealed class DemoApplications(ApplicationStore store, TimeProvider clock) : IHostedService
{
    private static readonly (string Id, string Deal, string Sponsor, string Asset, string Region, decimal Loan, decimal Value, decimal Dscr, int Deals, int HoursAgo)[] Seed =
    [
        ("0b4c2e10-0001-4a00-8000-000000000001", "Pier 7 Cold Store", "Northwake Estates", "Industrial", "Harbourside", 4_200_000m, 7_800_000m, 1.62m, 10, 30),
        ("0b4c2e10-0002-4a00-8000-000000000002", "Linden Row Apartments", "Fernbridge Homes", "Multifamily", "Millbrook", 9_500_000m, 13_200_000m, 1.31m, 6, 52),
        ("0b4c2e10-0003-4a00-8000-000000000003", "Quayside Hotel Refit", "Saltmarsh Hospitality", "Hotel", "Harbourside", 6_000_000m, 7_600_000m, 1.12m, 3, 28),
        ("0b4c2e10-0004-4a00-8000-000000000004", "Eastvale Logistics Park", "Corran Developments", "Industrial", "Eastvale", 12_000_000m, 19_500_000m, 1.48m, 9, 4),
        ("0b4c2e10-0005-4a00-8000-000000000005", "Tannery Lane Offices", "Wrenfield Capital", "Office", "Northgate", 3_100_000m, 4_200_000m, 1.25m, 2, 40),
        ("0b4c2e10-0006-4a00-8000-000000000006", "Weir Street Retail", "Halden & Moss", "Retail", "OuterRing", 2_400_000m, 3_000_000m, 1.05m, 1, 46),
        ("0b4c2e10-0007-4a00-8000-000000000007", "Rope Walk Student Housing", "Kestrelmoor Living", "Multifamily", "Northgate", 7_750_000m, 12_400_000m, 1.55m, 11, 10),
        ("0b4c2e10-0008-4a00-8000-000000000008", "Gull Point Self Storage", "Ardent Yard", "Industrial", "OuterRing", 1_800_000m, 3_300_000m, 1.70m, 5, 22),
        ("0b4c2e10-0009-4a00-8000-000000000009", "Mill Race Mixed Use", "Fernbridge Homes", "MixedUse", "Millbrook", 5_200_000m, 7_100_000m, 1.28m, 6, 34),
        ("0b4c2e10-0010-4a00-8000-000000000010", "Beacon Yard Land Plot", "Tidewick Land", "Land", "Eastvale", 2_000_000m, 2_600_000m, 0.80m, 2, 16),
    ];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        foreach (var s in Seed)
        {
            store.TryAdd(new ApplicationSnapshot(Guid.Parse(s.Id), s.Deal, s.Sponsor, s.Asset, s.Region,
                Money.Of(s.Loan, "GBP"), Money.Of(s.Value, "GBP"), s.Dscr, s.Deals, now.AddHours(-s.HoursAgo)));
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
