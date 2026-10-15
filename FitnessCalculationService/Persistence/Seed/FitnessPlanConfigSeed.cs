using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Domain.Enums;

namespace FitnessCalculationService.Persistence.Seed;

public static class FitnessPlanConfigSeed
{
    public static IEnumerable<FitnessPlanConfig> GetSeeds()
    {
        return new List<FitnessPlanConfig>
        {
            // Lose Weight
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-000000000000"),
                PlanId = "LW-W",
                Goal = FitnessGoal.LoseWeight,
                Status = CalorieStatus.Weak,
                Name = "Lose Weight Plan - Weak",
                Description = "A gentle caloric deficit program focused on low-impact movement and metabolic recovery.",
                MinCalorie = 1200,
                MaxCalorie = 1800,
                EstimatedDurationWeeks = 12,
                WorkoutsPerWeek = 3,
                ProgramType = "Low-Impact Cardio + Bodyweight",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-000000000001"),
                PlanId = "LW-N",
                Goal = FitnessGoal.LoseWeight,
                Status = CalorieStatus.Normal,
                Name = "Lose Weight Plan - Normal",
                Description = "A balanced fat-loss program combining progressive resistance training and cardiovascular work.",
                MinCalorie = 1801,
                MaxCalorie = 2500,
                EstimatedDurationWeeks = 10,
                WorkoutsPerWeek = 4,
                ProgramType = "Cardio + Strength",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-000000000002"),
                PlanId = "LW-H",
                Goal = FitnessGoal.LoseWeight,
                Status = CalorieStatus.Hard,
                Name = "Lose Weight Plan - Hard",
                Description = "High-intensity athletic conditioning designed for maximum calorie expenditure and lean mass retention.",
                MinCalorie = 2501,
                MaxCalorie = 4500,
                EstimatedDurationWeeks = 8,
                WorkoutsPerWeek = 5,
                ProgramType = "HIIT + Strength Circuits",
                IsActive = true
            },

            // Get Fitter
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-000100000000"),
                PlanId = "GF-W",
                Goal = FitnessGoal.GetFitter,
                Status = CalorieStatus.Weak,
                Name = "Get Fitter Plan - Weak",
                Description = "Baseline aerobic and muscular conditioning to build daily physical vitality.",
                MinCalorie = 1200,
                MaxCalorie = 1800,
                EstimatedDurationWeeks = 8,
                WorkoutsPerWeek = 3,
                ProgramType = "Full-Body Conditioning",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-000100000001"),
                PlanId = "GF-N",
                Goal = FitnessGoal.GetFitter,
                Status = CalorieStatus.Normal,
                Name = "Get Fitter Plan - Normal",
                Description = "Dynamic functional fitness emphasizing stamina, mobility, and core strength.",
                MinCalorie = 1801,
                MaxCalorie = 2500,
                EstimatedDurationWeeks = 8,
                WorkoutsPerWeek = 4,
                ProgramType = "Functional Strength + Cardio",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-000100000002"),
                PlanId = "GF-H",
                Goal = FitnessGoal.GetFitter,
                Status = CalorieStatus.Hard,
                Name = "Get Fitter Plan - Hard",
                Description = "Comprehensive athletic performance conditioning targeting speed, power, and high work capacity.",
                MinCalorie = 2501,
                MaxCalorie = 4500,
                EstimatedDurationWeeks = 6,
                WorkoutsPerWeek = 5,
                ProgramType = "Athletic Performance",
                IsActive = true
            },

            // Gain Weight
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-000200000000"),
                PlanId = "GW-W",
                Goal = FitnessGoal.GainWeight,
                Status = CalorieStatus.Weak,
                Name = "Gain Weight Plan - Weak",
                Description = "Introductory hypertrophy and controlled surplus program to stimulate healthy mass accrual.",
                MinCalorie = 1200,
                MaxCalorie = 1800,
                EstimatedDurationWeeks = 12,
                WorkoutsPerWeek = 3,
                ProgramType = "Foundational Hypertrophy",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-000200000001"),
                PlanId = "GW-N",
                Goal = FitnessGoal.GainWeight,
                Status = CalorieStatus.Normal,
                Name = "Gain Weight Plan - Normal",
                Description = "Standard muscle hypertrophy split with progressive overload and optimal volume distribution.",
                MinCalorie = 1801,
                MaxCalorie = 2500,
                EstimatedDurationWeeks = 12,
                WorkoutsPerWeek = 4,
                ProgramType = "Hypertrophy Split",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-000200000002"),
                PlanId = "GW-H",
                Goal = FitnessGoal.GainWeight,
                Status = CalorieStatus.Hard,
                Name = "Gain Weight Plan - Hard",
                Description = "High-volume strength and muscle-building program supported by high caloric demand.",
                MinCalorie = 2501,
                MaxCalorie = 4500,
                EstimatedDurationWeeks = 10,
                WorkoutsPerWeek = 5,
                ProgramType = "Progressive Overload Strength",
                IsActive = true
            },

            // Gain More Flexible
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-000300000000"),
                PlanId = "FL-W",
                Goal = FitnessGoal.GainMoreFlexible,
                Status = CalorieStatus.Weak,
                Name = "Gain More Flexible Plan - Weak",
                Description = "Gentle joint mobility and active stretching routines for improved range of motion.",
                MinCalorie = 1200,
                MaxCalorie = 1800,
                EstimatedDurationWeeks = 6,
                WorkoutsPerWeek = 3,
                ProgramType = "Mobility + Stretching",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-000300000001"),
                PlanId = "FL-N",
                Goal = FitnessGoal.GainMoreFlexible,
                Status = CalorieStatus.Normal,
                Name = "Gain More Flexible Plan - Normal",
                Description = "Vinyasa-inspired mobility flows combined with stability exercises for supple joints.",
                MinCalorie = 1801,
                MaxCalorie = 2500,
                EstimatedDurationWeeks = 6,
                WorkoutsPerWeek = 4,
                ProgramType = "Yoga Flow + Mobility",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-000300000002"),
                PlanId = "FL-H",
                Goal = FitnessGoal.GainMoreFlexible,
                Status = CalorieStatus.Hard,
                Name = "Gain More Flexible Plan - Hard",
                Description = "Challenging flexibility training integrating dynamic strength, end-range control, and deep isometric holds.",
                MinCalorie = 2501,
                MaxCalorie = 4500,
                EstimatedDurationWeeks = 8,
                WorkoutsPerWeek = 5,
                ProgramType = "Power Yoga + Pilates",
                IsActive = true
            },

            // Learn the Basic
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-000400000000"),
                PlanId = "LB-W",
                Goal = FitnessGoal.LearnTheBasic,
                Status = CalorieStatus.Weak,
                Name = "Learn the Basic Plan - Weak",
                Description = "Essential fundamental movement patterns (squat, hinge, push, pull) with low physical strain.",
                MinCalorie = 1200,
                MaxCalorie = 1800,
                EstimatedDurationWeeks = 4,
                WorkoutsPerWeek = 2,
                ProgramType = "Intro Bodyweight Basics",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-000400000001"),
                PlanId = "LB-N",
                Goal = FitnessGoal.LearnTheBasic,
                Status = CalorieStatus.Normal,
                Name = "Learn the Basic Plan - Normal",
                Description = "Comprehensive introduction to gym equipment, resistance training form, and training cadence.",
                MinCalorie = 1801,
                MaxCalorie = 2500,
                EstimatedDurationWeeks = 4,
                WorkoutsPerWeek = 3,
                ProgramType = "Fundamentals Full-Body",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-000400000002"),
                PlanId = "LB-H",
                Goal = FitnessGoal.LearnTheBasic,
                Status = CalorieStatus.Hard,
                Name = "Learn the Basic Plan - Hard",
                Description = "Accelerated foundation bootcamp establishing high motor competency and general physical preparedness.",
                MinCalorie = 2501,
                MaxCalorie = 4500,
                EstimatedDurationWeeks = 6,
                WorkoutsPerWeek = 4,
                ProgramType = "Fundamentals + Conditioning",
                IsActive = true
            }
        };
    }
}
