using System.Collections.Concurrent;
using Metis.Intake.Contracts;

namespace Metis.Intake.Data;

/// <summary>Thread-safe in-memory store. Slice 2 replaces it with a database adapter behind the same members.</summary>
internal sealed class ApplicationStore : IApplicationReader
{
    private readonly ConcurrentDictionary<Guid, ApplicationSnapshot> _items = new();

    public bool TryAdd(ApplicationSnapshot application) => _items.TryAdd(application.Id, application);

    public ApplicationSnapshot? Find(Guid id) => _items.TryGetValue(id, out var found) ? found : null;

    public Task<IReadOnlyList<ApplicationSnapshot>> ListOpenAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ApplicationSnapshot>>([.. _items.Values]);
}
