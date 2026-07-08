using AuthenticationService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthenticationService.Data.Configurations;

public class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Email);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(6);
        builder.Property(e => e.ExpiresAt).IsRequired();
        builder.Property(e => e.IsUsed).IsRequired();
    }
}
