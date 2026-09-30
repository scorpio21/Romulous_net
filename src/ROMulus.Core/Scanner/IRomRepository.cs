using ROMulus.Core.Models;

namespace ROMulus.Core.Scanner;

/// <summary>
/// ROM persistence contract consumed by <see cref="LibraryScanner"/>.
/// Implemented by <c>ROMulus.Infrastructure.Database.RomRepository</c>.
/// Keeping this interface in Core maintains the dependency rule:
/// Core → no reference to Infrastructure.
/// </summary>
public interface IRomRepository
{
    Dictionary<string, string> GetAliasBySystem();
    Dictionary<string, HashSet<string>> GetExtensionsBySystem();
    long InsertScanHistory(string scanType, DateTimeOffset startedAt, string rootPath);
    void UpdateScanHistory(long scanId, DateTimeOffset finishedAt,
        int filesFound, int filesMatched, int filesNew, int errors);
    long UpsertRom(Rom rom);
    int MarkMissingUnderRoot(string libraryRoot, IReadOnlySet<long> visitedIds,
        string? scopeSystemId = null);
}
