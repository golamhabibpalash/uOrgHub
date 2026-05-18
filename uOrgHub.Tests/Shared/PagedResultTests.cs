using FluentAssertions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Tests.Shared;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_rounds_up()
    {
        var result = new PagedResult<string> { TotalCount = 11, PageSize = 5, Page = 1 };
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public void TotalPages_exact_division()
    {
        var result = new PagedResult<string> { TotalCount = 10, PageSize = 5, Page = 1 };
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public void TotalPages_is_one_for_empty_result()
    {
        var result = new PagedResult<string> { TotalCount = 0, PageSize = 10, Page = 1 };
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public void HasNext_is_true_when_more_pages_exist()
    {
        var result = new PagedResult<string> { TotalCount = 25, PageSize = 10, Page = 1 };
        result.HasNext.Should().BeTrue();
    }

    [Fact]
    public void HasNext_is_false_on_last_page()
    {
        var result = new PagedResult<string> { TotalCount = 20, PageSize = 10, Page = 2 };
        result.HasNext.Should().BeFalse();
    }

    [Fact]
    public void HasPrevious_is_false_on_first_page()
    {
        var result = new PagedResult<string> { TotalCount = 50, PageSize = 10, Page = 1 };
        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void HasPrevious_is_true_on_second_page()
    {
        var result = new PagedResult<string> { TotalCount = 50, PageSize = 10, Page = 2 };
        result.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public void Items_defaults_to_empty_list()
    {
        var result = new PagedResult<int>();
        result.Items.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Single_page_result_has_no_next_or_previous()
    {
        var result = new PagedResult<string> { TotalCount = 3, PageSize = 10, Page = 1 };
        result.HasNext.Should().BeFalse();
        result.HasPrevious.Should().BeFalse();
    }
}
