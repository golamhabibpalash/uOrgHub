namespace uOrgHub.Projects.Models.Enums;

public enum ProjectStatus { Inquiry, Planning, Active, OnHold, Completed, Cancelled, Tender, Handover }
public enum ProjectPriority { Low, Medium, High, Critical }

public static class ProjectStatusTransition
{
    private static readonly HashSet<(ProjectStatus, ProjectStatus)> _allowed = new()
    {
        (ProjectStatus.Inquiry,    ProjectStatus.Planning),
        (ProjectStatus.Inquiry,    ProjectStatus.Tender),
        (ProjectStatus.Inquiry,    ProjectStatus.Cancelled),
        (ProjectStatus.Planning,   ProjectStatus.Active),
        (ProjectStatus.Planning,   ProjectStatus.OnHold),
        (ProjectStatus.Planning,   ProjectStatus.Cancelled),
        (ProjectStatus.Tender,     ProjectStatus.Planning),
        (ProjectStatus.Tender,     ProjectStatus.Active),
        (ProjectStatus.Tender,     ProjectStatus.Cancelled),
        (ProjectStatus.Active,     ProjectStatus.OnHold),
        (ProjectStatus.Active,     ProjectStatus.Completed),
        (ProjectStatus.Active,     ProjectStatus.Handover),
        (ProjectStatus.Active,     ProjectStatus.Cancelled),
        (ProjectStatus.OnHold,     ProjectStatus.Active),
        (ProjectStatus.OnHold,     ProjectStatus.Cancelled),
        (ProjectStatus.Handover,   ProjectStatus.Completed),
    };

    public static bool IsValid(ProjectStatus from, ProjectStatus to)
    {
        if (from == to) return true;
        return _allowed.Contains((from, to));
    }

    public static ProjectStatus[] ValidTargets(ProjectStatus from)
    {
        return _allowed
            .Where(t => t.Item1 == from)
            .Select(t => t.Item2)
            .Prepend(from)
            .Distinct()
            .ToArray();
    }
}
