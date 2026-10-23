using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Data.Configurations;

public class UserBiometricsConfiguration : IEntityTypeConfiguration<UserBiometrics>
{
    public void Configure(EntityTypeBuilder<UserBiometrics> builder)
    {
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.HeightCm)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();
    }
}
