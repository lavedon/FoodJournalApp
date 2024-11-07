using System.Globalization;
using FoodJournalApp.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FoodJournalApp;

public class Program
{
    public static void Main(string[] args)
    {
        var inputFile = "calories.yaml";
            var yamlContent = File.ReadAllText(inputFile);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yamlObject = deserializer.Deserialize<Dictionary<object, object>>(yamlContent);

            var caloriesData = new CaloriesData
            {
                DailyEntries = new Dictionary<string, Dictionary<string, FoodEntry>>()
            };

            foreach (var kvp in yamlObject)
            {
                string key = kvp.Key.ToString() ?? "";

                if (key == "daily_goal")
                {
                    caloriesData.DailyGoal = Convert.ToInt32(kvp.Value);
                }
                else if (key == "weekly_goal")
                {
                    caloriesData.WeeklyGoal = Convert.ToInt32(kvp.Value);
                }
                else
                {
                    var date = key;
                    var foodYaml = new SerializerBuilder().Build().Serialize(kvp.Value);
                    var foods = deserializer.Deserialize<Dictionary<string, FoodEntry>>(foodYaml);
                    caloriesData.DailyEntries[date] = foods;
                }
            }

            var dailyCalories = new Dictionary<string, int>();

            foreach (var entry in caloriesData.DailyEntries)
            {
                string date = entry.Key;
                var foods = entry.Value;

                int totalCalories = foods.Sum(food => food.Value.Quantity * food.Value.Calories);
                dailyCalories[date] = totalCalories;
            }

            bool calculateWeekly = args.Contains("--weekly");

            if (calculateWeekly)
            {
                var weeklyCalories = new Dictionary<int, WeeklyData>();

                foreach (var entry in dailyCalories)
                {
                    DateTime date = DateTime.Parse(entry.Key);
                    var calendar = CultureInfo.InvariantCulture.Calendar;
                    int week = calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                    if (!weeklyCalories.ContainsKey(week))
                    {
                        DateTime startOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)DayOfWeek.Monday);
                        DateTime endOfWeek = startOfWeek.AddDays(6);

                        weeklyCalories[week] = new WeeklyData
                        {
                            TotalCalories = 0,
                            StartDate = startOfWeek,
                            EndDate = endOfWeek
                        };
                    }

                    weeklyCalories[week].TotalCalories += entry.Value;
                }

                foreach (var entry in weeklyCalories.OrderBy(w => w.Key))
                {
                    int weekNumber = entry.Key;
                    var data = entry.Value;

                    string startDateStr = data.StartDate.ToString("yyyy-MM-dd");
                    string endDateStr = data.EndDate.ToString("yyyy-MM-dd");

                    Console.Write($"Week {weekNumber} ({startDateStr} to {endDateStr}): {data.TotalCalories} calories");

                    if (caloriesData.WeeklyGoal.HasValue)
                    {
                        int remainingCalories = caloriesData.WeeklyGoal.Value - data.TotalCalories;
                        Console.Write($" (Remaining: {remainingCalories} calories)");

                        if (remainingCalories < 0)
                        {
                            Console.Write(" [Over the goal!]");
                        }
                    }

                    Console.WriteLine();
                }

                // Optional: Serialize weekly data including goals
            }
            else
            {
                foreach (var entry in dailyCalories.OrderBy(d => d.Key))
                {
                    string date = entry.Key;
                    int consumedCalories = entry.Value;

                    Console.Write($"{date}: {consumedCalories} calories");

                    if (caloriesData.DailyGoal.HasValue)
                    {
                        int remainingCalories = caloriesData.DailyGoal.Value - consumedCalories;
                        Console.Write($" (Remaining: {remainingCalories} calories)");

                        if (remainingCalories < 0)
                        {
                            Console.Write(" [Over the goal!]");
                        }
                    }

                    Console.WriteLine();
                }

                // Optional: Serialize daily data including goals
            }
    }
}
