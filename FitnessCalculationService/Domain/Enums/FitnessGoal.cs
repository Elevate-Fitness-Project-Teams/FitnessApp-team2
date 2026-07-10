using System.ComponentModel;

namespace FitnessCalculationService.Domain.Enums;

public enum FitnessGoal
{
    [Description("Lose Weight")]
    LoseWeight,
    
    [Description("Get Fitter")]
    GetFitter,
    
    [Description("Gain Weight")]
    GainWeight,
    
    [Description("Gain More Flexible")]
    GainMoreFlexible,
    
    [Description("Learn The Basic")]
    LearnTheBasic
}
