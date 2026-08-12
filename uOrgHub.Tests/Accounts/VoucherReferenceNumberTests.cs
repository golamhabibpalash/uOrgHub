using FluentAssertions;
using uOrgHub.Accounts.DTOs.Validators;
using uOrgHub.Accounts.DTOs.Voucher;
using uOrgHub.Accounts.Mappings;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Tests.Accounts;

/// <summary>
/// The reference number is the number written on the physical voucher slip. It is optional free
/// text and never identifies the voucher — VoucherNumber still does that — so what matters is
/// only that it survives the round trip and that its length is bounded.
/// </summary>
public class VoucherReferenceNumberTests
{
    private readonly VoucherMapper _mapper = new();

    private static CreateVoucherDto CreateDto(string? reference) => new()
    {
        VoucherType = VoucherType.Debit,
        VoucherDate = new DateTime(2026, 8, 12),
        ProjectId = Guid.NewGuid(),
        Description = "Site materials",
        DebitAccountId = Guid.NewGuid(),
        CreditAccountId = Guid.NewGuid(),
        Amount = 5000m,
        ReferenceNumber = reference,
    };

    private static UpdateVoucherDto UpdateDto(string? reference) => new()
    {
        VoucherDate = new DateTime(2026, 8, 12),
        ProjectId = Guid.NewGuid(),
        Description = "Site materials",
        DebitAccountId = Guid.NewGuid(),
        CreditAccountId = Guid.NewGuid(),
        Amount = 5000m,
        ReferenceNumber = reference,
    };

    // --- Mapping ---
    // Mapperly maps by name, but the generated source is not checked in, so these assert the
    // mapping actually happens rather than trusting that it was wired up.

    [Fact]
    public void Create_maps_the_reference_onto_the_entity()
    {
        var entity = _mapper.ToEntity(CreateDto("PV-000123"));

        entity.ReferenceNumber.Should().Be("PV-000123");
    }

    [Fact]
    public void Update_maps_the_reference_onto_the_entity()
    {
        var entity = _mapper.ToEntity(CreateDto("PV-000123"));

        _mapper.UpdateEntity(UpdateDto("PV-999"), entity);

        entity.ReferenceNumber.Should().Be("PV-999");
    }

    /// <summary>Clearing the field on edit has to clear it on the record, not keep the old value.</summary>
    [Fact]
    public void Update_can_clear_the_reference()
    {
        var entity = _mapper.ToEntity(CreateDto("PV-000123"));

        _mapper.UpdateEntity(UpdateDto(null), entity);

        entity.ReferenceNumber.Should().BeNull();
    }

    [Fact]
    public void Response_carries_the_reference_back()
    {
        var entity = _mapper.ToEntity(CreateDto("PV-000123"));
        entity.VoucherNumber = "DR-2026-0001";
        // ToDto reads AccountName off both sides, and they are non-nullable navigations that the
        // query loads via Include. A bare entity has to supply them or the mapping faults.
        entity.DebitAccount = new ChartOfAccount { AccountCode = "5010", AccountName = "Materials" };
        entity.CreditAccount = new ChartOfAccount { AccountCode = "1010", AccountName = "Cash" };

        var dto = _mapper.ToDto(entity);

        dto.ReferenceNumber.Should().Be("PV-000123");
        // The system's own number is untouched by any of this.
        dto.VoucherNumber.Should().Be("DR-2026-0001");
    }

    // --- Validation ---

    [Fact]
    public void Reference_is_optional()
    {
        new CreateVoucherValidator().Validate(CreateDto(null)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Reference_within_50_characters_passes()
    {
        new CreateVoucherValidator().Validate(CreateDto(new string('X', 50))).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Reference_over_50_characters_fails()
    {
        new CreateVoucherValidator().Validate(CreateDto(new string('X', 51))).IsValid.Should().BeFalse();
        new UpdateVoucherValidator().Validate(UpdateDto(new string('X', 51))).IsValid.Should().BeFalse();
    }
}
