using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserProfileService.Models;

namespace UserProfileService.Common.Database.Configurations
{
    public class PrivacySettingsConfiguration : IEntityTypeConfiguration<PrivacySettings>
    {
        public void Configure(EntityTypeBuilder<PrivacySettings> builder)
        {
            builder.HasKey(ps => ps.Id);
            builder.Property(ps => ps.Id)
                  .ValueGeneratedNever(); // Relies on UserProfile.Id as both PK and FK

            builder.Property(ps => ps.ProfileVisibility)
                  .IsRequired()
                  .HasMaxLength(20)
                  .HasDefaultValue("private");

            builder.Property(ps => ps.ShowProgressToFriends)
                  .IsRequired()
                  .HasDefaultValue(false);

            builder.Property(ps => ps.AllowDataSharing)
                  .IsRequired()
                  .HasDefaultValue(false);

            // Configures 1:1 relationship where UserProfile is the Principal and PrivacySettings is the Dependent
            builder.HasOne(ps => ps.UserProfile)
                  .WithOne(u => u.PrivacySettings)
                  .HasForeignKey<PrivacySettings>(ps => ps.Id)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
