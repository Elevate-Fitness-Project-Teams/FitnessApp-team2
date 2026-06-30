namespace WorkoutService.Data.Entities;

public class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public string TargetMuscles { get; set; } = string.Empty;
    public string EquipmentNeeded { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;

    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
}
