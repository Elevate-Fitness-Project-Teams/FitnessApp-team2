using FitnessCalculationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Persistence;

public class FceDbContext : DbContext
{
    public FceDbContext(DbContextOptions<FceDbContext> options) : base(options) { }

    public DbSet<UserFitnessStats> UserFitnessStats { get; set; } = null!;
    public DbSet<CalculatedMetrics> CalculatedMetrics { get; set; } = null!;
    public DbSet<FitnessPlanConfig> FitnessPlanConfigs { get; set; } = null!;
    public DbSet<UserAssignedPlan> UserAssignedPlans { get; set; } = null!;
    public DbSet<UserPlanHistory> UserPlanHistories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FceDbContext).Assembly);
    }
}
