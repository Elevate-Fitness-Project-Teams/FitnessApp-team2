using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessCalculationService.Persistence.Seed;

public class FitnessPlanConfigSeed : IEntityTypeConfiguration<FitnessPlanConfig>
{
    public void Configure(EntityTypeBuilder<FitnessPlanConfig> builder)
    {
        var seeds = new List<FitnessPlanConfig>();

        foreach (FitnessGoal goal in Enum.GetValues(typeof(FitnessGoal)))
        {
            foreach (CalorieStatus status in Enum.GetValues(typeof(CalorieStatus)))
            {
                var planId = $"{GetGoalPrefix(goal)}-{status.ToString().Substring(0, 1)}";

                seeds.Add(new FitnessPlanConfig
                {
                    // Use a deterministic GUID so EF Core doesn't detect model changes on every run
                    Id = Guid.Parse($"11111111-1111-1111-1111-{(int)goal:D4}{(int)status:D8}"),
                    PlanId = planId,
                    Goal = goal,
                    Status = status,
                    Name = $"{goal} Plan - {status}",
                    Description = $"A plan designed for {goal} with a {status} metabolic rate."
                });
            }
        }

        builder.HasData(seeds);
    }

    private string GetGoalPrefix(FitnessGoal goal) => goal switch
    {
        FitnessGoal.LoseWeight => "LW",
        FitnessGoal.GetFitter => "GF",
        FitnessGoal.GainWeight => "GW",
        FitnessGoal.GainMoreFlexible => "FL",
        FitnessGoal.LearnTheBasic => "LB",
        _ => "DF"
    };
}
