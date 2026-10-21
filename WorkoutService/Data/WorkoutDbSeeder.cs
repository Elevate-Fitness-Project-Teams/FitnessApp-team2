using Microsoft.EntityFrameworkCore;
using WorkoutService.Data.Entities;

namespace WorkoutService.Data;

public static class WorkoutDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        if (await context.Exercises.AnyAsync())
        {
            return;
        }

        var benchPress = new Exercise
        {
            Id = Guid.Parse("22222222-2222-2222-2222-000000000001"),
            Name = "Barbell Bench Press",
            Description = "Lie on a flat bench, grip the barbell slightly wider than shoulder-width, lower the bar to mid-chest, and press explosively upwards.",
            VideoUrl = "https://cdn.elevatefitness.com/videos/bench-press.mp4",
            TargetMuscles = "Chest, Front Deltoids, Triceps",
            EquipmentNeeded = "Barbell, Flat Bench, Weight Plates",
            Difficulty = "Intermediate"
        };

        var backSquat = new Exercise
        {
            Id = Guid.Parse("22222222-2222-2222-2222-000000000002"),
            Name = "Barbell Back Squat",
            Description = "Rest the bar on upper trapezius, brace core, hinge hips back and down until thighs reach parallel, then drive upwards through mid-foot.",
            VideoUrl = "https://cdn.elevatefitness.com/videos/back-squat.mp4",
            TargetMuscles = "Quadriceps, Glutes, Hamstrings, Core",
            EquipmentNeeded = "Barbell, Squat Rack",
            Difficulty = "Intermediate"
        };

        var deadlift = new Exercise
        {
            Id = Guid.Parse("22222222-2222-2222-2222-000000000003"),
            Name = "Conventional Deadlift",
            Description = "Hinge at the hips with flat spine, grip the bar just outside knees, engage lats, and push the floor away to full hip extension.",
            VideoUrl = "https://cdn.elevatefitness.com/videos/deadlift.mp4",
            TargetMuscles = "Hamstrings, Glutes, Spinal Erectors, Traps",
            EquipmentNeeded = "Barbell, Weight Plates",
            Difficulty = "Advanced"
        };

        var overheadPress = new Exercise
        {
            Id = Guid.Parse("22222222-2222-2222-2222-000000000004"),
            Name = "Standing Overhead Press",
            Description = "Hold the barbell at collarbone level with elbows slightly forward, squeeze glutes and core, and press directly overhead to lockout.",
            VideoUrl = "https://cdn.elevatefitness.com/videos/overhead-press.mp4",
            TargetMuscles = "Shoulders, Upper Chest, Triceps, Core",
            EquipmentNeeded = "Barbell",
            Difficulty = "Intermediate"
        };

        var pullUp = new Exercise
        {
            Id = Guid.Parse("22222222-2222-2222-2222-000000000005"),
            Name = "Pull-Up",
            Description = "Hang from overhead bar with palms facing away, pull chest towards bar by depressing and retracting shoulder blades.",
            VideoUrl = "https://cdn.elevatefitness.com/videos/pull-up.mp4",
            TargetMuscles = "Latissimus Dorsi, Biceps, Upper Back",
            EquipmentNeeded = "Pull-Up Bar",
            Difficulty = "Intermediate"
        };

        await context.Exercises.AddRangeAsync(benchPress, backSquat, deadlift, overheadPress, pullUp);

        var workoutPlan = new WorkoutPlan
        {
            Id = Guid.Parse("22222222-2222-2222-2222-000000000010"),
            ExternalPlanId = "PLAN-FB-101",
            Name = "Full Body Strength Foundation",
            Description = "Progressive overload routine targeting major movement patterns to build foundational strength and muscle mass.",
            Goal = "Build Muscle",
            Status = "Active",
            Difficulty = "Intermediate"
        };

        var workout = new Workout
        {
            Id = Guid.Parse("22222222-2222-2222-2222-000000000020"),
            WorkoutPlanId = workoutPlan.Id,
            Name = "Full Body Power Day A",
            Category = "Strength",
            Difficulty = "Intermediate",
            DurationInMinutes = 50,
            CaloriesBurn = 380,
            ImageUrl = "https://images.unsplash.com/photo-1517838277536-f5f99be501cd",
            IsPremium = false
        };

        var workoutExercises = new List<WorkoutExercise>
        {
            new()
            {
                Id = Guid.NewGuid(),
                WorkoutId = workout.Id,
                ExerciseId = backSquat.Id,
                OrderIndex = 1,
                SetsDefault = 4,
                RepsDefault = "8-10",
                RestTimeInSeconds = 90
            },
            new()
            {
                Id = Guid.NewGuid(),
                WorkoutId = workout.Id,
                ExerciseId = benchPress.Id,
                OrderIndex = 2,
                SetsDefault = 4,
                RepsDefault = "8-10",
                RestTimeInSeconds = 90
            },
            new()
            {
                Id = Guid.NewGuid(),
                WorkoutId = workout.Id,
                ExerciseId = pullUp.Id,
                OrderIndex = 3,
                SetsDefault = 3,
                RepsDefault = "8-12",
                RestTimeInSeconds = 60
            },
            new()
            {
                Id = Guid.NewGuid(),
                WorkoutId = workout.Id,
                ExerciseId = overheadPress.Id,
                OrderIndex = 4,
                SetsDefault = 3,
                RepsDefault = "10",
                RestTimeInSeconds = 60
            }
        };

        await context.WorkoutPlans.AddAsync(workoutPlan);
        await context.Workouts.AddAsync(workout);
        await context.WorkoutExercises.AddRangeAsync(workoutExercises);

        await context.SaveChangesAsync();
        logger.LogInformation("Successfully seeded catalog exercises, default plan, and workout session template in WorkoutService.");
    }
}
