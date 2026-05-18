using FluentAssertions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Tests.Shared;

public class PaginationRequestTests
{
    [Fact]
    public void Default_page_is_1()
    {
        new PaginationRequest().Page.Should().Be(1);
    }

    [Fact]
    public void Default_page_size_is_10()
    {
        new PaginationRequest().PageSize.Should().Be(10);
    }

    [Fact]
    public void Page_below_1_clamps_to_1()
    {
        var req = new PaginationRequest { Page = 0 };
        req.Page.Should().Be(1);
    }

    [Fact]
    public void Negative_page_clamps_to_1()
    {
        var req = new PaginationRequest { Page = -5 };
        req.Page.Should().Be(1);
    }

    [Fact]
    public void Valid_page_is_stored()
    {
        var req = new PaginationRequest { Page = 5 };
        req.Page.Should().Be(5);
    }

    [Fact]
    public void PageSize_below_1_defaults_to_10()
    {
        var req = new PaginationRequest { PageSize = 0 };
        req.PageSize.Should().Be(10);
    }

    [Fact]
    public void PageSize_above_100_clamps_to_100()
    {
        var req = new PaginationRequest { PageSize = 200 };
        req.PageSize.Should().Be(100);
    }

    [Fact]
    public void PageSize_exactly_100_is_stored()
    {
        var req = new PaginationRequest { PageSize = 100 };
        req.PageSize.Should().Be(100);
    }

    [Fact]
    public void PageSize_1_is_stored()
    {
        var req = new PaginationRequest { PageSize = 1 };
        req.PageSize.Should().Be(1);
    }

    [Fact]
    public void SortDescending_defaults_to_false()
    {
        new PaginationRequest().SortDescending.Should().BeFalse();
    }

    [Fact]
    public void Search_defaults_to_null()
    {
        new PaginationRequest().Search.Should().BeNull();
    }
}
