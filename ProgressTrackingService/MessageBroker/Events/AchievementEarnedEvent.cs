namespace MessageBroker.Events;

public class AchievementEarnedEvent
{
    public Guid UserId { get; set; }
    public Guid AchievementId { get; set; }
    public string AchievementName { get; set; } = string.Empty;
    public DateTime EarnedAt { get; set; }
}
