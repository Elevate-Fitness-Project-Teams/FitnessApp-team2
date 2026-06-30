using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserProfileService.Models;

namespace UserProfileService.Common.Database.Configurations
{
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.HasKey(up => up.Id);
            builder.Property(up => up.Id)
                  .ValueGeneratedNever(); // Primary key is provided by Authentication Service, not database-generated.

            builder.Property(up => up.FirstName)
                  .IsRequired()
                  .HasMaxLength(50);

            builder.Property(up => up.LastName)
                  .IsRequired()
                  .HasMaxLength(50);

            builder.Property(up => up.Email)
                  .IsRequired()
                  .HasMaxLength(255);

            builder.Property(up => up.PhoneNumber)
                  .IsRequired()
                  .HasMaxLength(20);

            builder.Property(up => up.ProfilePictureUrl)
                  .HasMaxLength(500)
                  .IsRequired(false);

            builder.Property(up => up.IsPremiumCached)
                  .IsRequired()
                  .HasDefaultValue(false);

            builder.Property(up => up.MemberSince)
                  .IsRequired();
        }
    }
}
