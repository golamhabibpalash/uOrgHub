using FluentAssertions;
using uOrgHub.HR.Features.Attendance;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;

namespace uOrgHub.Tests.HR;

public class AttendanceShiftEvaluatorTests
{
    // Asia/Dhaka is UTC+6 with no DST, so local 09:00 = 03:00 UTC.
    private static readonly TimeZoneInfo Dhaka = AttendanceShiftEvaluator.ResolveTimeZone("Asia/Dhaka");
    private static readonly DateTime Day = new(2026, 10, 7);

    private static EmployeeRoster Roster(TimeSpan start, TimeSpan end, int graceMinutes = 10, bool isOff = false, bool flexible = false) => new()
    {
        RosterDate = Day,
        IsOff = isOff,
        Shift = new Shift
        {
            Name = "General", StartTime = start, EndTime = end,
            WorkSchedule = new WorkSchedule { Name = "Office", GracePeriodMinutes = graceMinutes, IsFlexible = flexible }
        }
    };

    private static AttendanceLog Log(string checkInUtc, string? checkOutUtc, AttendanceStatus status = AttendanceStatus.Present) => new()
    {
        AttendanceDate = Day,
        CheckIn = DateTime.SpecifyKind(DateTime.Parse(checkInUtc), DateTimeKind.Utc),
        CheckOut = checkOutUtc == null ? null : DateTime.SpecifyKind(DateTime.Parse(checkOutUtc), DateTimeKind.Utc),
        Status = status
    };

    [Fact]
    public void Check_in_within_grace_stays_present()
    {
        var log = Log("2026-10-07 03:08", "2026-10-07 12:00"); // 09:08 local, grace 10
        AttendanceShiftEvaluator.Apply(log, Roster(new(9, 0, 0), new(18, 0, 0)), Dhaka, applyLateRule: true);

        log.Status.Should().Be(AttendanceStatus.Present);
        log.WorkHours.Should().Be(8.87m);
        log.OvertimeHours.Should().Be(0);
    }

    [Fact]
    public void Check_in_after_grace_is_marked_late()
    {
        var log = Log("2026-10-07 03:45", "2026-10-07 12:00"); // 09:45 local
        AttendanceShiftEvaluator.Apply(log, Roster(new(9, 0, 0), new(18, 0, 0)), Dhaka, applyLateRule: true);

        log.Status.Should().Be(AttendanceStatus.Late);
    }

    [Fact]
    public void Late_rule_is_skipped_when_not_requested()
    {
        var log = Log("2026-10-07 03:45", "2026-10-07 12:00");
        AttendanceShiftEvaluator.Apply(log, Roster(new(9, 0, 0), new(18, 0, 0)), Dhaka, applyLateRule: false);

        log.Status.Should().Be(AttendanceStatus.Present);
    }

    [Fact]
    public void Flexible_schedule_is_never_late()
    {
        var log = Log("2026-10-07 05:00", "2026-10-07 13:00");
        AttendanceShiftEvaluator.Apply(log, Roster(new(9, 0, 0), new(18, 0, 0), flexible: true), Dhaka, applyLateRule: true);

        log.Status.Should().Be(AttendanceStatus.Present);
    }

    [Fact]
    public void Non_present_status_is_not_overridden()
    {
        var log = Log("2026-10-07 05:00", "2026-10-07 09:00", AttendanceStatus.HalfDay);
        AttendanceShiftEvaluator.Apply(log, Roster(new(9, 0, 0), new(18, 0, 0)), Dhaka, applyLateRule: true);

        log.Status.Should().Be(AttendanceStatus.HalfDay);
    }

    [Fact]
    public void Hours_beyond_shift_length_are_overtime()
    {
        var log = Log("2026-10-07 03:00", "2026-10-07 14:30"); // 09:00–20:30 local on a 9h shift
        AttendanceShiftEvaluator.Apply(log, Roster(new(9, 0, 0), new(18, 0, 0)), Dhaka, applyLateRule: true);

        log.WorkHours.Should().Be(11.5m);
        log.OvertimeHours.Should().Be(2.5m);
    }

    [Fact]
    public void Night_shift_ends_the_next_day()
    {
        // 22:00–06:00 local = 16:00 UTC on the 7th to 00:00 UTC on the 8th.
        var (start, end) = AttendanceShiftEvaluator.ShiftWindowUtc(Day, new Shift { StartTime = new(22, 0, 0), EndTime = new(6, 0, 0) }, Dhaka);

        start.Should().Be(new DateTime(2026, 10, 7, 16, 0, 0, DateTimeKind.Utc));
        end.Should().Be(new DateTime(2026, 10, 8, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Work_on_a_rostered_day_off_is_all_overtime()
    {
        var log = Log("2026-10-07 03:00", "2026-10-07 07:00");
        AttendanceShiftEvaluator.Apply(log, Roster(new(9, 0, 0), new(18, 0, 0), isOff: true), Dhaka, applyLateRule: true);

        log.OvertimeHours.Should().Be(4m);
        log.Status.Should().Be(AttendanceStatus.Present);
    }

    [Fact]
    public void Without_a_roster_only_work_hours_are_set()
    {
        var log = Log("2026-10-07 05:00", "2026-10-07 15:00");
        AttendanceShiftEvaluator.Apply(log, null, Dhaka, applyLateRule: true);

        log.WorkHours.Should().Be(10m);
        log.OvertimeHours.Should().Be(0);
        log.Status.Should().Be(AttendanceStatus.Present);
    }
}
