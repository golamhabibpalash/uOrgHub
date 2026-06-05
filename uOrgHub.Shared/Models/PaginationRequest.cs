using System.Text.Json;

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

    private string? _filtersJson;
    public string? FiltersJson
    {
        get => _filtersJson;
        set
        {
            _filtersJson = value;
            _filters = null;
        }
    }

    private Dictionary<string, string>? _filters;
    public Dictionary<string, string> Filters
    {
        get
        {
            if (_filters == null)
            {
                if (!string.IsNullOrWhiteSpace(_filtersJson))
                {
                    try { _filters = JsonSerializer.Deserialize<Dictionary<string, string>>(_filtersJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new(); }
                    catch { _filters = new(); }
                }
                else
                    _filters = new();
            }
            return _filters;
        }
        set => _filters = value;
    }
}