using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.FixedAssets;
using uOrgHub.Accounts.Features.FixedAssets;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Reporting.ExportColumns;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/asset-categories")]
public class AssetCategoriesController : BaseController
{
    private readonly IMediator _mediator;
    public AssetCategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Accounts.AssetCategories.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetAssetCategoriesQuery(request));
        return Ok(ApiResponse<PagedResult<AssetCategoryResponseDto>>.Ok(result));
    }

    /// <summary>Unpaged list for pickers (the fixed-asset form's category dropdown).</summary>
    [HttpGet("all")]
    [RequireClaim(Claims.Accounts.FixedAssets.View)]
    public async Task<IActionResult> GetAllUnpaged()
    {
        var result = await _mediator.Send(new GetAllAssetCategoriesQuery());
        return Ok(ApiResponse<List<AssetCategoryResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.AssetCategories.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetAssetCategoryByIdQuery(id));
        return Ok(ApiResponse<AssetCategoryResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.AssetCategories.Create)]
    public async Task<IActionResult> Create([FromBody] CreateAssetCategoryDto dto)
    {
        var result = await _mediator.Send(new CreateAssetCategoryCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<AssetCategoryResponseDto>.Ok(result, "Asset category created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.AssetCategories.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAssetCategoryDto dto)
    {
        var result = await _mediator.Send(new UpdateAssetCategoryCommand(id, dto));
        return Ok(ApiResponse<AssetCategoryResponseDto>.Ok(result, "Asset category updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Accounts.AssetCategories.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteAssetCategoryCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Asset category deleted successfully."));
    }
}

[Authorize]
[Route("api/v1/accounts/fixed-assets")]
public class FixedAssetsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public FixedAssetsController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Accounts.FixedAssets.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? categoryId, [FromQuery] FixedAssetStatus? status)
    {
        var result = await _mediator.Send(new GetFixedAssetsQuery(request, categoryId, status));
        return Ok(ApiResponse<PagedResult<FixedAssetResponseDto>>.Ok(result));
    }

    [HttpGet("summary")]
    [RequireClaim(Claims.Accounts.FixedAssets.View)]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _mediator.Send(new GetFixedAssetSummaryQuery());
        return Ok(ApiResponse<FixedAssetSummaryDto>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Accounts.FixedAssets.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllFixedAssetsForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, FixedAssetExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "FixedAssets"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.FixedAssets.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetFixedAssetByIdQuery(id));
        return Ok(ApiResponse<FixedAssetResponseDto>.Ok(result));
    }

    [HttpGet("{id:guid}/depreciation-history")]
    [RequireClaim(Claims.Accounts.FixedAssets.View)]
    public async Task<IActionResult> GetDepreciationHistory(Guid id)
    {
        var result = await _mediator.Send(new GetFixedAssetDepreciationHistoryQuery(id));
        return Ok(ApiResponse<List<FixedAssetDepreciationHistoryDto>>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.FixedAssets.Create)]
    public async Task<IActionResult> Create([FromBody] CreateFixedAssetDto dto)
    {
        var result = await _mediator.Send(new CreateFixedAssetCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<FixedAssetResponseDto>.Ok(result, "Fixed asset registered successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.FixedAssets.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFixedAssetDto dto)
    {
        var result = await _mediator.Send(new UpdateFixedAssetCommand(id, dto));
        return Ok(ApiResponse<FixedAssetResponseDto>.Ok(result, "Fixed asset updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Accounts.FixedAssets.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteFixedAssetCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Fixed asset deleted successfully."));
    }
}

[Authorize]
[Route("api/v1/accounts/depreciation-runs")]
public class DepreciationRunsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public DepreciationRunsController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Accounts.Depreciation.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] int? year)
    {
        var result = await _mediator.Send(new GetDepreciationRunsQuery(request, year));
        return Ok(ApiResponse<PagedResult<DepreciationRunResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Accounts.Depreciation.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllDepreciationRunsForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, DepreciationRunExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "DepreciationRuns"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    /// <summary>What a run for this month would charge, without posting anything.</summary>
    [HttpGet("preview")]
    [RequireClaim(Claims.Accounts.Depreciation.View)]
    public async Task<IActionResult> Preview([FromQuery] int year, [FromQuery] int month)
    {
        var result = await _mediator.Send(new PreviewDepreciationQuery(year, month));
        return Ok(ApiResponse<DepreciationPreviewDto>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.Depreciation.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDepreciationRunByIdQuery(id));
        return Ok(ApiResponse<DepreciationRunResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.Depreciation.Post)]
    public async Task<IActionResult> Post([FromBody] PostDepreciationRunDto dto)
    {
        var result = await _mediator.Send(new PostDepreciationRunCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<DepreciationRunResponseDto>.Ok(result, $"Depreciation {result.RunNumber} posted for {result.AssetCount} asset(s)."));
    }

    [HttpPost("{id:guid}/reverse")]
    [RequireClaim(Claims.Accounts.Depreciation.Reverse)]
    public async Task<IActionResult> Reverse(Guid id)
    {
        var result = await _mediator.Send(new ReverseDepreciationRunCommand(id));
        return Ok(ApiResponse<DepreciationRunResponseDto>.Ok(result, $"Depreciation {result.RunNumber} reversed."));
    }
}
