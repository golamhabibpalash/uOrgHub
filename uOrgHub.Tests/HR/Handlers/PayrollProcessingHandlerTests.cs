using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Payroll;
using uOrgHub.HR.Features.Payroll.Commands;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Tests.HR.Handlers;

public class PayrollProcessingHandlerTests
{
    private readonly AppDbContext _db = TestDb.NewContext();
    private readonly SalaryGrade _grade = new() { GradeCode = "G1", Name = "Officer", MinSalary = 20000, MaxSalary = 60000 };
    private readonly SalaryComponent _hra = new() { Name = "House Rent", Code = "HRA", ComponentType = SalaryComponentType.HouseRentAllowance, CalculationType = CalculationType.PercentageOfBasic };
    private readonly SalaryComponent _pf = new() { Name = "Provident Fund", Code = "PF", ComponentType = SalaryComponentType.PF, CalculationType = CalculationType.PercentageOfBasic, SortOrder = 5 };
    private readonly PayrollCycle _cycle = new() { Year = 2026, Month = 9, Title = "September 2026", StartDate = new(2026, 9, 1), EndDate = new(2026, 9, 30) };

    private readonly Department _dept = new() { Name = "Engineering", Code = "ENG" };
    private readonly Designation _desig;

    public PayrollProcessingHandlerTests()
    {
        _desig = new Designation { Name = "Engineer", Code = "EN", Level = 1, Department = _dept };
        _db.AddRange(_grade, _hra, _pf, _cycle, _dept, _desig);
        _db.SaveChanges();
    }

    private Employee AddEmployee(string code, decimal basic = 0, EmployeeStatus status = EmployeeStatus.Active)
    {
        var e = new Employee
        {
            EmployeeCode = code, FirstName = code, LastName = "Test", Email = $"{code}@test.com",
            JoiningDate = new DateTime(2025, 1, 1), BasicSalary = basic, Status = status,
            DepartmentId = _dept.Id, DesignationId = _desig.Id
        };
        _db.Add(e);
        _db.SaveChanges();
        return e;
    }

    private Task<EmployeeSalaryStructureResponseDto> CreateStructure(Guid employeeId, decimal basic, DateTime effective) =>
        new CreateEmployeeSalaryStructureCommandHandler(_db).Handle(new CreateEmployeeSalaryStructureCommand(new CreateEmployeeSalaryStructureDto
        {
            EmployeeId = employeeId, SalaryGradeId = _grade.Id, BasicSalary = basic, EffectiveDate = effective,
            Components = [new() { SalaryComponentId = _hra.Id, Value = 50 }, new() { SalaryComponentId = _pf.Id, Value = 10 }]
        }), CancellationToken.None);

    private Task<ProcessPayrollResultDto> Process() =>
        new ProcessPayrollCycleCommandHandler(_db).Handle(new ProcessPayrollCycleCommand(_cycle.Id), CancellationToken.None);

    [Fact]
    public async Task Structure_computes_gross_and_syncs_employee_salary()
    {
        var emp = AddEmployee("E1");

        var result = await CreateStructure(emp.Id, 30000, new DateTime(2026, 1, 1));

        result.GrossSalary.Should().Be(45000);
        result.NetSalary.Should().Be(42000);
        (await _db.Set<Employee>().FindAsync(emp.Id))!.BasicSalary.Should().Be(30000);
    }

    [Fact]
    public async Task New_structure_closes_the_previous_one()
    {
        var emp = AddEmployee("E1");
        var first = await CreateStructure(emp.Id, 30000, new DateTime(2026, 1, 1));

        await CreateStructure(emp.Id, 35000, new DateTime(2026, 7, 1));

        var old = await _db.Set<EmployeeSalaryStructure>().FindAsync(first.Id);
        old!.IsActive.Should().BeFalse();
        old.EndDate.Should().Be(new DateTime(2026, 6, 30));
    }

    [Fact]
    public async Task Structure_cannot_start_on_or_before_the_current_one()
    {
        var emp = AddEmployee("E1");
        await CreateStructure(emp.Id, 30000, new DateTime(2026, 7, 1));

        var act = () => CreateStructure(emp.Id, 35000, new DateTime(2026, 7, 1));

        await act.Should().ThrowAsync<AppException>().WithMessage("*Edit it*");
    }

    [Fact]
    public async Task Process_creates_payslips_from_structures_and_attendance()
    {
        var withStructure = AddEmployee("E1");
        await CreateStructure(withStructure.Id, 30000, new DateTime(2026, 1, 1));
        var basicOnly = AddEmployee("E2", basic: 20000);
        AddEmployee("E3"); // no salary at all
        AddEmployee("E4", basic: 25000, status: EmployeeStatus.Terminated);
        _db.Add(new AttendanceLog { EmployeeId = withStructure.Id, AttendanceDate = new DateTime(2026, 9, 10), Status = AttendanceStatus.Absent });
        await _db.SaveChangesAsync();

        var result = await Process();

        result.ProcessedEmployees.Should().Be(2);
        result.Skipped.Should().ContainSingle(s => s.Contains("E3"));
        result.Cycle.Status.Should().Be(PayrollStatus.Processed);

        var e1 = await _db.Set<PayrollEntry>().Include(x => x.Lines).SingleAsync(x => x.EmployeeId == withStructure.Id);
        e1.GrossSalary.Should().Be(45000);
        e1.AbsentDays.Should().Be(1);
        e1.Lines.Should().Contain(l => l.Code == "UNPAID" && l.Amount == 1500); // 45,000 / 30
        e1.NetSalary.Should().Be(45000 - 3000 - 1500);

        var e2 = await _db.Set<PayrollEntry>().SingleAsync(x => x.EmployeeId == basicOnly.Id);
        e2.NetSalary.Should().Be(20000);
        result.Cycle.TotalNetPay.Should().Be(e1.NetSalary + 20000);
    }

    [Fact]
    public async Task Reprocessing_recalculates_in_place()
    {
        var emp = AddEmployee("E1", basic: 20000);
        await Process();
        emp.BasicSalary = 22000;
        await _db.SaveChangesAsync();

        await Process();

        var entries = await _db.Set<PayrollEntry>().Include(x => x.Lines).Where(x => x.EmployeeId == emp.Id).ToListAsync();
        entries.Should().ContainSingle().Which.NetSalary.Should().Be(22000);
        entries[0].Lines.Count(l => !l.IsDeleted).Should().Be(1);
    }

    [Fact]
    public async Task Approved_cycle_cannot_be_reprocessed()
    {
        AddEmployee("E1", basic: 20000);
        await Process();
        await new UpdatePayrollCycleCommandHandler(_db).Handle(
            new UpdatePayrollCycleCommand(_cycle.Id, new UpdatePayrollCycleDto { Status = PayrollStatus.Approved }), CancellationToken.None);

        var act = Process;

        await act.Should().ThrowAsync<AppException>();
        (await _db.Set<PayrollEntry>().SingleAsync()).Status.Should().Be(PayrollStatus.Approved);
    }

    [Fact]
    public async Task Draft_cannot_be_marked_processed_without_processing()
    {
        var act = () => new UpdatePayrollCycleCommandHandler(_db).Handle(
            new UpdatePayrollCycleCommand(_cycle.Id, new UpdatePayrollCycleDto { Status = PayrollStatus.Processed }), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>().WithMessage("*Process*");
    }

    [Theory]
    [InlineData(PayrollStatus.Processed, PayrollStatus.Approved, true)]
    [InlineData(PayrollStatus.Approved, PayrollStatus.Paid, true)]
    [InlineData(PayrollStatus.Paid, PayrollStatus.Processed, false)]
    [InlineData(PayrollStatus.Draft, PayrollStatus.Paid, false)]
    public void Cycle_transitions(PayrollStatus from, PayrollStatus to, bool allowed) =>
        PayrollCycleTransitions.IsAllowed(from, to).Should().Be(allowed);

    [Fact]
    public async Task Deleted_month_can_be_created_again()
    {
        await new DeletePayrollCycleCommandHandler(_db).Handle(new DeletePayrollCycleCommand(_cycle.Id), CancellationToken.None);

        var recreated = await new CreatePayrollCycleCommandHandler(_db).Handle(new CreatePayrollCycleCommand(new CreatePayrollCycleDto
        {
            Year = 2026, Month = 9, Title = "Sep 2026 (redo)", StartDate = new(2026, 9, 1), EndDate = new(2026, 9, 30)
        }), CancellationToken.None);

        recreated.Title.Should().Be("Sep 2026 (redo)");
        recreated.Status.Should().Be(PayrollStatus.Draft);
    }

    [Fact]
    public async Task Editing_structure_replaces_its_components()
    {
        var emp = AddEmployee("E1");
        var created = await CreateStructure(emp.Id, 30000, new DateTime(2026, 1, 1));

        var updated = await new UpdateEmployeeSalaryStructureCommandHandler(_db).Handle(new UpdateEmployeeSalaryStructureCommand(created.Id,
            new UpdateEmployeeSalaryStructureDto
            {
                SalaryGradeId = _grade.Id, BasicSalary = 32000,
                Components = [new() { SalaryComponentId = _hra.Id, Value = 40 }]  // PF removed
            }), CancellationToken.None);

        updated.Components.Should().ContainSingle(c => c.Code == "HRA" && c.Amount == 12800);
        updated.GrossSalary.Should().Be(44800);
        updated.NetSalary.Should().Be(44800);
    }
}
