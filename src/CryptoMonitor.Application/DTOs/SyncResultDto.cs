namespace CryptoMonitor.Application.DTOs;

public record SyncResultDto(
    int AssetsSynced,
    DateTimeOffset SyncedAt
);
