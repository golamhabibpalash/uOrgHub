using FluentAssertions;
using Moq;
using uOrgHub.HR.DTOs.Attendance;
using uOrgHub.HR.Features.Attendance.Commands;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Entities;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Services;

namespace uOrgHub.Tests.HR.Handlers;

public class AttendanceCommandHandlerTests
{
    private static readonly DateTime Day = new(2026, 10, 7);
    private readonly AppDbContext _db = TestDb.NewContext();
    private readonly Mock<ICurrentCompanyAccessor> _company = new();
    private readonly Employee _employee;
    private readonly Shift _shift;

    public AttendanceCommandHandlerTests()
    {
        _db.Set<Company>().Add(new Company { Name = "Main", TimeZone = "Asia/Dhaka" });
        _employee = new Employee { EmployeeCode = "EMP001", FirstName = "Rahim", LastName = "Uddin", Email = "rahim@test.com" };
        var schedule = new WorkSchedule { Name = "Head Office", TotalHours = 8, GracePeriodMinutes = 10 };
        _shift = new Shift { Name = "General", Code = "GEN", StartTime = new(9, 0, 0), EndTime = new(18, 0, 0), WorkSchedule = schedule };
        _db.AddRange(_employee, schedule, _shift);
        _db.SaveChanges();
    }

    private Task<EmployeeRosterResponseDto> Roster(DateTime date) =>
        new CreateEmployeeRosterCommandHandler(_db).Handle(
            new CreateEmployeeRosterCommand(new CreateEmployeeRosterDto { EmployeeId = _employee.Id, ShiftId = _shift.Id, RosterDate = date }),
            CancellationToken.None);

    private Task<AttendanceLogResponseDto> CreateLog(string checkInUtc, string checkOutUtc) =>
        new CreateAttendanceLogCommandHandler(_db, _company.Object).Handle(
            new CreateAttendanceLogCommand(new CreateAttendanceLogDto
            {
                EmployeeId = _employee.Id, AttendanceDate = Day,
                CheckIn = DateTime.SpecifyKind(DateTime.Parse(checkInUtc), DateTimeKind.Utc),
                CheckOut = DateTime.SpecifyKind(DateTime.Parse(checkOutUtc), DateTimeKind.Utc),
            }),
            CancellationToken.None);

    [Fact]
    public async Task Roster_twice_for_same_day_is_rejected()
    {
        await Roster(Day);

        var act = () => Roster(Day);

        await act.Should().ThrowAsync<AppException>().WithMessage("*already rostered*");
    }

    [Fact]
    public async Task Roster_for_unknown_shift_is_not_found()
    {
        var act = () => new CreateEmployeeRosterCommandHandler(_db).Handle(
            new CreateEmployeeRosterCommand(new CreateEmployeeRosterDto { EmployeeId = _employee.Id, ShiftId = Guid.NewGuid(), RosterDate = Day }),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Log_on_rostered_day_is_judged_against_the_shift()
    {
        await Roster(Day);

        // 09:45–20:00 Dhaka time on a 09:00–18:00 shift with 10 min grace.
        var result = await CreateLog("2026-10-07 03:45", "2026-10-07 14:00");

        result.Status.Should().Be(AttendanceStatus.Late);
        result.ShiftName.Should().Be("General");
        result.WorkHours.Should().Be(10.25m);
        result.OvertimeHours.Should().Be(1.25m);
    }

    [Fact]
    public async Task Log_without_roster_keeps_chosen_status()
    {
        var result = await CreateLog("2026-10-07 03:45", "2026-10-07 14:00");

        result.Status.Should().Be(AttendanceStatus.Present);
        result.ShiftName.Should().BeNull();
        result.OvertimeHours.Should().Be(0);
    }

    [Fact]
    public async Task Editing_log_without_changing_check_in_keeps_hr_status()
    {
        await Roster(Day);
        var created = await CreateLog("2026-10-07 03:45", "2026-10-07 12:00");

        // HR excuses the late arrival: status back to Present, same check-in.
        var updated = await new UpdateAttendanceLogCommandHandler(_db, _company.Object).Handle(
            new UpdateAttendanceLogCommand(created.Id, new UpdateAttendanceLogDto
            {
                CheckIn = created.CheckIn, CheckOut = created.CheckOut,
                Status = AttendanceStatus.Present, Remarks = "Excused"
            }),
            CancellationToken.None);

        updated.Status.Should().Be(AttendanceStatus.Present);
    }

    [Fact]
    public async Task Shift_code_must_stay_unique_on_update()
    {
        var other = new Shift { Name = "Night", Code = "NGT", StartTime = new(22, 0, 0), EndTime = new(6, 0, 0), WorkScheduleId = _shift.WorkScheduleId };
        _db.Add(other);
        await _db.SaveChangesAsync();

        var act = () => new UpdateShiftCommandHandler(_db).Handle(
            new UpdateShiftCommand(other.Id, new UpdateShiftDto
            {
                Name = "Night", Code = "GEN", WorkScheduleId = _shift.WorkScheduleId,
                StartTime = new(22, 0, 0), EndTime = new(6, 0, 0)
            }),
            CancellationToken.None);

        await act.Should().ThrowAsync<AppException>().WithMessage("*GEN*already exists*");
    }

    [Fact]
    public async Task Work_schedule_update_saves_policy_fields()
    {
        var ws = _shift.WorkSchedule;

        var result = await new UpdateWorkScheduleCommandHandler(_db).Handle(
            new UpdateWorkScheduleCommand(ws.Id, new UpdateWorkScheduleDto
            {
                Name = "Head Office", StartTime = new(9, 0, 0), EndTime = new(18, 0, 0),
                TotalHours = 9, GracePeriodMinutes = 15, WorkingDaysPerWeek = 6
            }),
            CancellationToken.None);

        result.GracePeriodMinutes.Should().Be(15);
        result.WorkingDaysPerWeek.Should().Be(6);
        result.StartTime.Should().Be(new TimeSpan(9, 0, 0));
    }
}
