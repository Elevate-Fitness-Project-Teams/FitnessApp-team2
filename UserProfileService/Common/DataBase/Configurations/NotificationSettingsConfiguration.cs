using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserProfileService.Models;

namespace UserProfileService.Common.Database.Configurations
{
    public class NotificationSettingsConfiguration : IEntityTypeConfiguration<NotificationSettings>
    {
        public void Configure(EntityTypeBuilder<NotificationSettings> builder)
        {
            builder.HasKey(ns => ns.Id);
            builder.Property(ns => ns.Id)
                  .ValueGeneratedNever(); // Relies on UserProfile.Id as both PK and FK

            builder.Property(ns => ns.WorkoutReminders)
                  .IsRequired()
                  .HasDefaultValue(true);

            builder.Property(ns => ns.MealReminders)
                  .IsRequired()
                  .HasDefaultValue(true);

            builder.Property(ns => ns.AchievementAlerts)
                  .IsRequired()
                  .HasDefaultValue(true);

            builder.Property(ns => ns.WeeklyReports)
                  .IsRequired()
                  .HasDefaultValue(true);

            builder.Property(ns => ns.EmailNotifications)
                  .IsRequired()
                  .HasDefaultValue(true);

            builder.Property(ns => ns.PushNotifications)
                  .IsRequired()
                  .HasDefaultValue(true);

            // Configures 1:1 relationship where UserProfile is the Principal and NotificationSettings is the Dependent
            builder.HasOne(ns => ns.UserProfile)
                  .WithOne(u => u.NotificationSettings)
                  .HasForeignKey<NotificationSettings>(ns => ns.Id)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
