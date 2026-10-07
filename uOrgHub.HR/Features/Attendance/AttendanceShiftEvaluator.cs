using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;

namespace uOrgHub.HR.Features.Attendance;

/// <summary>
/// Applies the employee's rostered shift to an attendance log: work hours, overtime beyond the
/// shift's length, and Late when check-in falls after shift start + the schedule's grace period.
/// Shift times are company-local wall-clock times, while CheckIn/CheckOut are stored in UTC, so the
/// shift window is converted through the company's time zone before comparing.
/// </summary>
public static class AttendanceShiftEvaluator
{
    /// <param name="applyLateRule">
    /// False keeps the status the user chose — an edit that doesn't touch check-in must not undo an
    /// HR decision to excuse a late arrival.
    /// </param>
    public static void Apply(AttendanceLog log, EmployeeRoster? roster, TimeZoneInfo timeZone, bool applyLateRule)
    {
        log.WorkHours = log.CheckIn.HasValue && log.CheckOut.HasValue
            ? Math.Round((decimal)(log.CheckOut.Value - log.CheckIn.Value).TotalHours, 2)
            : 0;
        log.OvertimeHours = 0;

        if (roster?.Shift == null) return;

        // Any hours worked on a rostered day off are overtime.
        if (roster.IsOff)
        {
            log.OvertimeHours = log.WorkHours;
            return;
        }

        var (shiftStartUtc, shiftEndUtc) = ShiftWindowUtc(log.AttendanceDate, roster.Shift, timeZone);
        var shiftHours = (decimal)(shiftEndUtc - shiftStartUtc).TotalHours;
        log.OvertimeHours = Math.Max(0, Math.Round(log.WorkHours - shiftHours, 2));

        var schedule = roster.Shift.WorkSchedule;
        if (applyLateRule
            && log.Status == AttendanceStatus.Present
            && log.CheckIn.HasValue
            && schedule is not { IsFlexible: true })
        {
            var graceMinutes = schedule?.GracePeriodMinutes ?? 0;
            if (log.CheckIn.Value > shiftStartUtc.AddMinutes(graceMinutes))
                log.Status = AttendanceStatus.Late;
        }
    }

    /// <summary>
    /// The shift's start and end on the attendance date, in UTC. A shift whose end is not after its
    /// start (e.g. 22:00–06:00) finishes the next day.
    /// </summary>
    public static (DateTime StartUtc, DateTime EndUtc) ShiftWindowUtc(DateTime attendanceDate, Shift shift, TimeZoneInfo timeZone)
    {
        var day = DateTime.SpecifyKind(attendanceDate.Date, DateTimeKind.Unspecified);
        var startLocal = day + shift.StartTime;
        var endLocal = day + shift.EndTime;
        if (shift.EndTime <= shift.StartTime) endLocal = endLocal.AddDays(1);

        return (TimeZoneInfo.ConvertTimeToUtc(startLocal, timeZone),
                TimeZoneInfo.ConvertTimeToUtc(endLocal, timeZone));
    }

    public static TimeZoneInfo ResolveTimeZone(string? timeZoneId) =>
        !string.IsNullOrWhiteSpace(timeZoneId) && TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var tz)
            ? tz
            : TimeZoneInfo.Utc;
}
