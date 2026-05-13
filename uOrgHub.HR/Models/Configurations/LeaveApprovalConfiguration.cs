using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class LeaveApprovalConfiguration : IEntityTypeConfiguration<LeaveApproval>
{
    public void Configure(EntityTypeBuilder<LeaveApproval> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.LeaveRequest).WithMany(x => x.Approvals)
         .HasForeignKey(x => x.LeaveRequestId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Approver).WithMany()
         .HasForeignKey(x => x.ApproverId).OnDelete(DeleteBehavior.Restrict);
    }
}
