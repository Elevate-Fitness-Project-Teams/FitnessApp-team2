using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserProfileService.Models;

namespace UserProfileService.Common.Database.Configurations
{
    public class UserPreferencesConfiguration : IEntityTypeConfiguration<UserPreferences>
    {
        public void Configure(EntityTypeBuilder<UserPreferences> builder)
        {
            builder.HasKey(up => up.Id);
            builder.Property(up => up.Id)
                  .ValueGeneratedNever(); // Relies on UserProfile.Id as both PK and FK

            builder.Property(up => up.Language)
                  .IsRequired()
                  .HasMaxLength(10)
                  .HasDefaultValue("en");

            builder.Property(up => up.Theme)
                  .IsRequired()
                  .HasMaxLength(15)
                  .HasDefaultValue("light");

            builder.Property(up => up.WeightUnit)
                  .IsRequired()
                  .HasMaxLength(5)
                  .HasDefaultValue("kg");

            builder.Property(up => up.HeightUnit)
                  .IsRequired()
                  .HasMaxLength(5)
                  .HasDefaultValue("cm");

            builder.Property(up => up.DistanceUnit)
                  .IsRequired()
                  .HasMaxLength(5)
                  .HasDefaultValue("km");

            // Configures 1:1 relationship where UserProfile is the Principal and UserPreferences is the Dependent
            builder.HasOne(up => up.UserProfile)
                  .WithOne(u => u.UserPreferences)
                  .HasForeignKey<UserPreferences>(up => up.Id)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
