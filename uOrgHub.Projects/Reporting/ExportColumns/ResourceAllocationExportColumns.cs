using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class ResourceAllocationExportColumns
{
    public static List<ExportColumn<ResourceAllocationResponseDto>> Get() =>
    [
        new("resourceType", "Resource Type", x => x.ResourceType.ToString()),
        new("description", "Description", x => x.Description),
        new("equipmentCode", "Equipment Code", x => x.EquipmentCode),
        new("unitOfMeasure", "UOM", x => x.UnitOfMeasure),
        new("plannedStartDate", "Planned Start", x => x.PlannedStartDate),
        new("plannedEndDate", "Planned End", x => x.PlannedEndDate),
        new("actualStartDate", "Actual Start", x => x.ActualStartDate),
        new("actualEndDate", "Actual End", x => x.ActualEndDate),
        new("plannedQuantity", "Planned Qty", x => x.PlannedQuantity),
        new("actualQuantity", "Actual Qty", x => x.ActualQuantity),
        new("unitCost", "Unit Cost", x => x.UnitCost),
        new("plannedCost", "Planned Cost", x => x.PlannedCost),
        new("actualCost", "Actual Cost", x => x.ActualCost),
        new("status", "Status", x => x.Status.ToString()),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
