namespace FoodJournalApp.Models;

public class CaloriesData
{
    public int? DailyGoal { get; set; }
    public int? WeeklyGoal { get; set; }
    public Dictionary<string, Dictionary<string, FoodEntry>> DailyEntries { get; set; } = new();
}
