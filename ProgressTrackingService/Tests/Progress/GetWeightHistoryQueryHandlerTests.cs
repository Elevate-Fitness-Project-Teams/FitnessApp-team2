using Microsoft.Extensions.Logging.Abstractions;
using ProgressTrackingService.Common.Caching;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Common.Errors;
using ProgressTrackingService.Features.Progress.Queries.GetWeightHistory;
using ProgressTrackingService.Models;
using ProgressTrackingService.Tests.Caching;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.Progress;

public class GetWeightHistoryQueryHandlerTests : IDisposable
{
    private readonly SqliteTestDbContext _db;
    private readonly GeneralRepo<WeightHistory> _repo;
    private readonly TestDistributedCache _cache;
    private readonly RedisCacheService _cacheService;
    private readonly CacheInvalidator _cacheInvalidator;

    public GetWeightHistoryQueryHandlerTests()
    {
        _db = new SqliteTestDbContext();
        _repo = new GeneralRepo<WeightHistory>(_db.Context);
        _cache = new TestDistributedCache();
        _cacheService = new RedisCacheService(_cache, NullLogger<RedisCacheService>.Instance);
        _cacheInvalidator = new CacheInvalidator(_cacheService, NullLogger<CacheInvalidator>.Instance);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task Handle_ReturnsEntriesInAscendingChronologicalOrder()
    {
        var userId = Guid.NewGuid();
        var baseDate = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);

        _db.Context.WeightHistories.AddRange(
            new WeightHistory { Id = Guid.NewGuid(), UserId = userId, Weight = 82.5, Date = baseDate.AddDays(5) },
            new WeightHistory { Id = Guid.NewGuid(), UserId = userId, Weight = 85.0, Date = baseDate },
            new WeightHistory { Id = Guid.NewGuid(), UserId = userId, Weight = 80.0, Date = baseDate.AddDays(10) }
        );
        await _db.Context.SaveChangesAsync();

        var handler = new GetWeightHistoryQueryHandler(
            _repo, _cacheService, _cacheInvalidator, NullLogger<GetWeightHistoryQueryHandler>.Instance);

        var result = await handler.Handle(new GetWeightHistoryQuery(userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalCount);
        Assert.Equal(3, result.Value.Items.Count);

        // Verify ASC ordering (85.0 -> 82.5 -> 80.0)
        Assert.Equal(85.0, result.Value.Items[0].Weight);
        Assert.Equal(82.5, result.Value.Items[1].Weight);
        Assert.Equal(80.0, result.Value.Items[2].Weight);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPageSlice()
    {
        var userId = Guid.NewGuid();
        var baseDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        for (int i = 0; i < 5; i++)
        {
            _db.Context.WeightHistories.Add(new WeightHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Weight = 70.0 + i,
                Date = baseDate.AddDays(i)
            });
        }
        await _db.Context.SaveChangesAsync();

        var handler = new GetWeightHistoryQueryHandler(
            _repo, _cacheService, _cacheInvalidator, NullLogger<GetWeightHistoryQueryHandler>.Instance);

        var query = new GetWeightHistoryQuery(userId, Page: 2, PageSize: 2);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(2, result.Value.Page);
        Assert.Equal(2, result.Value.PageSize);
        Assert.Equal(72.0, result.Value.Items[0].Weight); // index 2
        Assert.Equal(73.0, result.Value.Items[1].Weight); // index 3
    }

    [Fact]
    public async Task Handle_WithDateFilters_RestrictsToRange()
    {
        var userId = Guid.NewGuid();
        var baseDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        _db.Context.WeightHistories.AddRange(
            new WeightHistory { Id = Guid.NewGuid(), UserId = userId, Weight = 75.0, Date = baseDate.AddDays(-2) },
            new WeightHistory { Id = Guid.NewGuid(), UserId = userId, Weight = 76.0, Date = baseDate.AddDays(2) },
            new WeightHistory { Id = Guid.NewGuid(), UserId = userId, Weight = 77.0, Date = baseDate.AddDays(8) }
        );
        await _db.Context.SaveChangesAsync();

        var handler = new GetWeightHistoryQueryHandler(
            _repo, _cacheService, _cacheInvalidator, NullLogger<GetWeightHistoryQueryHandler>.Instance);

        var query = new GetWeightHistoryQuery(
            userId,
            From: baseDate,
            To: baseDate.AddDays(5));

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Single(result.Value.Items);
        Assert.Equal(76.0, result.Value.Items[0].Weight);
    }

    [Fact]
    public async Task Handle_WhenFromDateGreaterThanToDate_ReturnsValidationError()
    {
        var userId = Guid.NewGuid();
        var handler = new GetWeightHistoryQueryHandler(
            _repo, _cacheService, _cacheInvalidator, NullLogger<GetWeightHistoryQueryHandler>.Instance);

        var query = new GetWeightHistoryQuery(
            userId,
            From: DateTime.UtcNow.AddDays(5),
            To: DateTime.UtcNow);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProgressErrors.ValInvalidDate, result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenNoEntriesExist_ReturnsSuccessWithEmptyList()
    {
        var userId = Guid.NewGuid();
        var handler = new GetWeightHistoryQueryHandler(
            _repo, _cacheService, _cacheInvalidator, NullLogger<GetWeightHistoryQueryHandler>.Instance);

        var result = await handler.Handle(new GetWeightHistoryQuery(userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task Handle_CachesDefaultPagedResult()
    {
        var userId = Guid.NewGuid();
        _db.Context.WeightHistories.Add(new WeightHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Weight = 78.0,
            Date = DateTime.UtcNow
        });
        await _db.Context.SaveChangesAsync();

        var handler = new GetWeightHistoryQueryHandler(
            _repo, _cacheService, _cacheInvalidator, NullLogger<GetWeightHistoryQueryHandler>.Instance);

        // First call caches result
        var result1 = await handler.Handle(new GetWeightHistoryQuery(userId, Page: 1, PageSize: 10), CancellationToken.None);
        Assert.True(result1.IsSuccess);

        // Delete from database to verify second call serves from cache
        _db.Context.WeightHistories.RemoveRange(_db.Context.WeightHistories);
        await _db.Context.SaveChangesAsync();

        var result2 = await handler.Handle(new GetWeightHistoryQuery(userId, Page: 1, PageSize: 10), CancellationToken.None);
        Assert.True(result2.IsSuccess);
        Assert.Single(result2.Value.Items);
        Assert.Equal(78.0, result2.Value.Items[0].Weight);
    }
}
