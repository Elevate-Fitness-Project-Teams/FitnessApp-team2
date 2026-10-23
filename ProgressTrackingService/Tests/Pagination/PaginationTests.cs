using ProgressTrackingService.Common.Pagination;
using Xunit;

namespace ProgressTrackingService.Tests.Pagination;

public class PaginationTests
{
    [Fact]
    public void PagedQuery_Defaults_ToPage1AndSize20()
    {
        var query = new PagedQuery();
        Assert.Equal(1, query.Page);
        Assert.Equal(20, query.PageSize);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(10, 10)]
    public void PagedQuery_Clamps_InvalidPageValues(int input, int expected)
    {
        var query = new PagedQuery { Page = input };
        Assert.Equal(expected, query.Page);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-10, 20)]
    [InlineData(50, 50)]
    [InlineData(150, 100)] // max 100
    public void PagedQuery_Clamps_InvalidPageSizeValues(int input, int expected)
    {
        var query = new PagedQuery { PageSize = input };
        Assert.Equal(expected, query.PageSize);
    }

    [Fact]
    public void PagedResult_Calculates_TotalPagesAndNavigation()
    {
        var items = new[] { "item1", "item2" };
        var result = PagedResult<string>.Create(items, totalCount: 25, page: 2, pageSize: 10);

        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void PagedResult_EmptyItems_HandlesZeroCountGracefully()
    {
        var result = PagedResult<string>.Create(Array.Empty<string>(), totalCount: 0, page: 1, pageSize: 20);

        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
        Assert.Empty(result.Items);
    }
}
