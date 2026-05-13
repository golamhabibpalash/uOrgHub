namespace uOrgHub.Shared.Models;

public class PaginationRequest
{
    private int _pageSize = 10;
    private int _page = 1;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 100 ? 100 : value < 1 ? 10 : value;
    }

    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}