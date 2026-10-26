namespace ProgressTrackingService.Common.Domain;

public static class BmiCalculator
{
    /// <summary>
    /// Calculates Body Mass Index (BMI) = weight (kg) / [height (m)]^2.
    /// Returns null if height or weight is invalid or missing.
    /// </summary>
    public static double? Calculate(double weightKg, double? heightCm)
    {
        if (weightKg <= 0 || heightCm == null || heightCm.Value <= 0)
        {
            return null;
        }

        var heightMeters = heightCm.Value / 100.0;
        var bmi = weightKg / (heightMeters * heightMeters);

        return Math.Round(bmi, 1);
    }
}
