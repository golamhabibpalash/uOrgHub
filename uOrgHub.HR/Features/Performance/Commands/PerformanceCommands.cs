using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Performance;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.HR.Features.Performance.Commands;

public record CreateReviewCycleCommand(CreateReviewCycleDto Dto) : ICommand<ReviewCycleResponseDto>;
public record UpdateReviewCycleCommand(Guid Id, UpdateReviewCycleDto Dto) : ICommand<ReviewCycleResponseDto>;
public record CreateKPICommand(CreateKPIDto Dto) : ICommand<KPIResponseDto>;
public record CreateGoalCommand(CreateGoalDto Dto) : ICommand<GoalResponseDto>;
public record UpdateGoalCommand(Guid Id, UpdateGoalDto Dto) : ICommand<GoalResponseDto>;
public record CreatePerformanceReviewCommand(CreatePerformanceReviewDto Dto) : ICommand<PerformanceReviewResponseDto>;
public record SubmitPerformanceReviewCommand(Guid Id, UpdatePerformanceReviewDto Dto) : ICommand<PerformanceReviewResponseDto>;
public record CreateTrainingProgramCommand(CreateTrainingProgramDto Dto) : ICommand<TrainingProgramResponseDto>;
public record EnrollEmployeeTrainingCommand(CreateEmployeeTrainingDto Dto) : ICommand<EmployeeTrainingResponseDto>;
public record UpdateEmployeeTrainingCommand(Guid Id, UpdateEmployeeTrainingDto Dto) : ICommand<EmployeeTrainingResponseDto>;

public class CreateReviewCycleCommandHandler : IRequestHandler<CreateReviewCycleCommand, ReviewCycleResponseDto>
{
    private readonly AppDbContext _context;
    public CreateReviewCycleCommandHandler(AppDbContext context) => _context = context;

    public async Task<ReviewCycleResponseDto> Handle(CreateReviewCycleCommand request, CancellationToken ct)
    {
        var entity = new ReviewCycle
        {
            Name = request.Dto.Name, Type = request.Dto.Type,
            StartDate = request.Dto.StartDate, EndDate = request.Dto.EndDate,
            Status = ReviewStatus.Draft, Description = request.Dto.Description,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<ReviewCycle>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return PerformanceMappingHelper.MapCycleToDto(entity);
    }
}

public class UpdateReviewCycleCommandHandler : IRequestHandler<UpdateReviewCycleCommand, ReviewCycleResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateReviewCycleCommandHandler(AppDbContext context) => _context = context;

    public async Task<ReviewCycleResponseDto> Handle(UpdateReviewCycleCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ReviewCycle>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ReviewCycle), request.Id);

        entity.Name = request.Dto.Name; entity.Status = request.Dto.Status;
        if (request.Dto.EndDate.HasValue) entity.EndDate = request.Dto.EndDate.Value;
        entity.Description = request.Dto.Description; entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<ReviewCycle>().Update(entity);
        await _context.SaveChangesAsync(ct);
        return PerformanceMappingHelper.MapCycleToDto(entity);
    }
}

public class CreateKPICommandHandler : IRequestHandler<CreateKPICommand, KPIResponseDto>
{
    private readonly AppDbContext _context;
    public CreateKPICommandHandler(AppDbContext context) => _context = context;

    public async Task<KPIResponseDto> Handle(CreateKPICommand request, CancellationToken ct)
    {
        var entity = new KPI
        {
            Name = request.Dto.Name, Description = request.Dto.Description,
            MeasurementUnit = request.Dto.MeasurementUnit, TargetValue = request.Dto.TargetValue,
            Weight = request.Dto.Weight, IsActive = request.Dto.IsActive,
            DepartmentId = request.Dto.DepartmentId, DesignationId = request.Dto.DesignationId,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<KPI>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var dept = entity.DepartmentId.HasValue ? await _context.Set<Department>().FindAsync(entity.DepartmentId) : null;
        var desig = entity.DesignationId.HasValue ? await _context.Set<Designation>().FindAsync(entity.DesignationId) : null;
        return new KPIResponseDto
        {
            Id = entity.Id, Name = entity.Name, Description = entity.Description,
            MeasurementUnit = entity.MeasurementUnit, TargetValue = entity.TargetValue,
            Weight = entity.Weight, IsActive = entity.IsActive,
            DepartmentId = entity.DepartmentId, DepartmentName = dept?.Name,
            DesignationId = entity.DesignationId, DesignationName = desig?.Name,
            CreatedAt = entity.CreatedAt
        };
    }
}

public class CreateGoalCommandHandler : IRequestHandler<CreateGoalCommand, GoalResponseDto>
{
    private readonly AppDbContext _context;
    public CreateGoalCommandHandler(AppDbContext context) => _context = context;

    public async Task<GoalResponseDto> Handle(CreateGoalCommand request, CancellationToken ct)
    {
        var entity = new Goal
        {
            EmployeeId = request.Dto.EmployeeId, ReviewCycleId = request.Dto.ReviewCycleId,
            KPIId = request.Dto.KPIId, Title = request.Dto.Title,
            Description = request.Dto.Description, TargetValue = request.Dto.TargetValue,
            Weight = request.Dto.Weight, Status = GoalStatus.NotStarted,
            DueDate = request.Dto.DueDate, CreatedAt = DateTime.UtcNow
        };
        _context.Set<Goal>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(entity.EmployeeId);
        var cycle = await _context.Set<ReviewCycle>().FindAsync(entity.ReviewCycleId);
        var kpi = entity.KPIId.HasValue ? await _context.Set<KPI>().FindAsync(entity.KPIId) : null;
        return PerformanceMappingHelper.MapGoalToDto(entity, employee, cycle, kpi);
    }
}

public class UpdateGoalCommandHandler : IRequestHandler<UpdateGoalCommand, GoalResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateGoalCommandHandler(AppDbContext context) => _context = context;

    public async Task<GoalResponseDto> Handle(UpdateGoalCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Goal>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Goal), request.Id);

        entity.AchievedValue = request.Dto.AchievedValue;
        entity.Status = request.Dto.Status;
        entity.Remarks = request.Dto.Remarks;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<Goal>().Update(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(entity.EmployeeId);
        var cycle = await _context.Set<ReviewCycle>().FindAsync(entity.ReviewCycleId);
        var kpi = entity.KPIId.HasValue ? await _context.Set<KPI>().FindAsync(entity.KPIId) : null;
        return PerformanceMappingHelper.MapGoalToDto(entity, employee, cycle, kpi);
    }
}

public class CreatePerformanceReviewCommandHandler : IRequestHandler<CreatePerformanceReviewCommand, PerformanceReviewResponseDto>
{
    private readonly AppDbContext _context;
    public CreatePerformanceReviewCommandHandler(AppDbContext context) => _context = context;

    public async Task<PerformanceReviewResponseDto> Handle(CreatePerformanceReviewCommand request, CancellationToken ct)
    {
        var entity = new PerformanceReview
        {
            ReviewCycleId = request.Dto.ReviewCycleId, EmployeeId = request.Dto.EmployeeId,
            ReviewerId = request.Dto.ReviewerId, ReviewType = request.Dto.ReviewType,
            Status = ReviewStatus.Draft, DueDate = request.Dto.DueDate, CreatedAt = DateTime.UtcNow
        };
        _context.Set<PerformanceReview>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(entity.EmployeeId);
        var reviewer = await _context.Set<Employee>().FindAsync(entity.ReviewerId);
        var cycle = await _context.Set<ReviewCycle>().FindAsync(entity.ReviewCycleId);
        return PerformanceMappingHelper.MapReviewToDto(entity, employee, reviewer, cycle);
    }
}

public class SubmitPerformanceReviewCommandHandler : IRequestHandler<SubmitPerformanceReviewCommand, PerformanceReviewResponseDto>
{
    private readonly AppDbContext _context;
    public SubmitPerformanceReviewCommandHandler(AppDbContext context) => _context = context;

    public async Task<PerformanceReviewResponseDto> Handle(SubmitPerformanceReviewCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<PerformanceReview>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(PerformanceReview), request.Id);

        entity.OverallRating = request.Dto.OverallRating;
        entity.Comments = request.Dto.Comments; entity.Strengths = request.Dto.Strengths;
        entity.AreasForImprovement = request.Dto.AreasForImprovement;
        entity.Status = request.Dto.Status;
        if (request.Dto.Status == ReviewStatus.Completed) entity.SubmittedDate = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<PerformanceReview>().Update(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(entity.EmployeeId);
        var reviewer = await _context.Set<Employee>().FindAsync(entity.ReviewerId);
        var cycle = await _context.Set<ReviewCycle>().FindAsync(entity.ReviewCycleId);
        return PerformanceMappingHelper.MapReviewToDto(entity, employee, reviewer, cycle);
    }
}

public class CreateTrainingProgramCommandHandler : IRequestHandler<CreateTrainingProgramCommand, TrainingProgramResponseDto>
{
    private readonly AppDbContext _context;
    public CreateTrainingProgramCommandHandler(AppDbContext context) => _context = context;

    public async Task<TrainingProgramResponseDto> Handle(CreateTrainingProgramCommand request, CancellationToken ct)
    {
        var entity = new TrainingProgram
        {
            Title = request.Dto.Title, Description = request.Dto.Description,
            Category = request.Dto.Category, Mode = request.Dto.Mode,
            DurationHours = request.Dto.DurationHours, Provider = request.Dto.Provider,
            Location = request.Dto.Location, MaxParticipants = request.Dto.MaxParticipants,
            Cost = request.Dto.Cost, StartDate = request.Dto.StartDate, EndDate = request.Dto.EndDate,
            Status = TrainingStatus.Upcoming, HasCertificate = request.Dto.HasCertificate,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<TrainingProgram>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return PerformanceMappingHelper.MapTrainingToDto(entity);
    }
}

public class EnrollEmployeeTrainingCommandHandler : IRequestHandler<EnrollEmployeeTrainingCommand, EmployeeTrainingResponseDto>
{
    private readonly AppDbContext _context;
    public EnrollEmployeeTrainingCommandHandler(AppDbContext context) => _context = context;

    public async Task<EmployeeTrainingResponseDto> Handle(EnrollEmployeeTrainingCommand request, CancellationToken ct)
    {
        var exists = await _context.Set<EmployeeTraining>()
            .AnyAsync(x => !x.IsDeleted && x.EmployeeId == request.Dto.EmployeeId && x.TrainingProgramId == request.Dto.TrainingProgramId, ct);
        if (exists) throw new AppException("Employee is already enrolled in this training program.");

        var entity = new EmployeeTraining
        {
            EmployeeId = request.Dto.EmployeeId, TrainingProgramId = request.Dto.TrainingProgramId,
            EnrollmentDate = request.Dto.EnrollmentDate, Status = OnboardingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<EmployeeTraining>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(entity.EmployeeId);
        var program = await _context.Set<TrainingProgram>().FindAsync(entity.TrainingProgramId);
        return PerformanceMappingHelper.MapEmployeeTrainingToDto(entity, employee, program);
    }
}

public class UpdateEmployeeTrainingCommandHandler : IRequestHandler<UpdateEmployeeTrainingCommand, EmployeeTrainingResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateEmployeeTrainingCommandHandler(AppDbContext context) => _context = context;

    public async Task<EmployeeTrainingResponseDto> Handle(UpdateEmployeeTrainingCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<EmployeeTraining>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(EmployeeTraining), request.Id);

        entity.Status = request.Dto.Status; entity.CompletionDate = request.Dto.CompletionDate;
        entity.Score = request.Dto.Score; entity.CertificatePath = request.Dto.CertificatePath;
        entity.Remarks = request.Dto.Remarks; entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<EmployeeTraining>().Update(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(entity.EmployeeId);
        var program = await _context.Set<TrainingProgram>().FindAsync(entity.TrainingProgramId);
        return PerformanceMappingHelper.MapEmployeeTrainingToDto(entity, employee, program);
    }
}

file static class PerformanceMappingHelper
{
    internal static ReviewCycleResponseDto MapCycleToDto(ReviewCycle e) => new()
    {
        Id = e.Id, Name = e.Name, Type = e.Type, StartDate = e.StartDate,
        EndDate = e.EndDate, Status = e.Status, Description = e.Description, CreatedAt = e.CreatedAt
    };

    internal static GoalResponseDto MapGoalToDto(Goal e, Employee? emp, ReviewCycle? cycle, KPI? kpi) => new()
    {
        Id = e.Id, EmployeeId = e.EmployeeId,
        EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}" : string.Empty,
        ReviewCycleId = e.ReviewCycleId, ReviewCycleName = cycle?.Name ?? string.Empty,
        KPIId = e.KPIId, KPIName = kpi?.Name, Title = e.Title, Description = e.Description,
        TargetValue = e.TargetValue, AchievedValue = e.AchievedValue, Weight = e.Weight,
        Status = e.Status, DueDate = e.DueDate, Remarks = e.Remarks, CreatedAt = e.CreatedAt
    };

    internal static PerformanceReviewResponseDto MapReviewToDto(PerformanceReview e, Employee? emp, Employee? reviewer, ReviewCycle? cycle) => new()
    {
        Id = e.Id, ReviewCycleId = e.ReviewCycleId, ReviewCycleName = cycle?.Name ?? string.Empty,
        EmployeeId = e.EmployeeId,
        EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}" : string.Empty,
        ReviewerId = e.ReviewerId,
        ReviewerName = reviewer != null ? $"{reviewer.FirstName} {reviewer.LastName}" : string.Empty,
        ReviewType = e.ReviewType, OverallRating = e.OverallRating, Comments = e.Comments,
        Strengths = e.Strengths, AreasForImprovement = e.AreasForImprovement,
        Status = e.Status, DueDate = e.DueDate, SubmittedDate = e.SubmittedDate, CreatedAt = e.CreatedAt
    };

    internal static TrainingProgramResponseDto MapTrainingToDto(TrainingProgram e) => new()
    {
        Id = e.Id, Title = e.Title, Description = e.Description, Category = e.Category,
        Mode = e.Mode, DurationHours = e.DurationHours, Provider = e.Provider,
        Location = e.Location, MaxParticipants = e.MaxParticipants, Cost = e.Cost,
        StartDate = e.StartDate, EndDate = e.EndDate, Status = e.Status,
        HasCertificate = e.HasCertificate, CreatedAt = e.CreatedAt
    };

    internal static EmployeeTrainingResponseDto MapEmployeeTrainingToDto(EmployeeTraining e, Employee? emp, TrainingProgram? prog) => new()
    {
        Id = e.Id, EmployeeId = e.EmployeeId,
        EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}" : string.Empty,
        TrainingProgramId = e.TrainingProgramId, TrainingTitle = prog?.Title ?? string.Empty,
        EnrollmentDate = e.EnrollmentDate, CompletionDate = e.CompletionDate,
        Status = e.Status, Score = e.Score, CertificatePath = e.CertificatePath,
        Remarks = e.Remarks, CreatedAt = e.CreatedAt
    };
}
