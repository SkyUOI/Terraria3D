using System.Collections.Generic;

namespace Terraria3D.achievements;

public enum AchievementCategory
{
    None,
    Challenger,
    Collector,
    Explorer,
    Slayer,
}

public record AchievementData(
    string Name,
    AchievementCategory Category,
    string Description
);

public static class AchievementRegistry
{
    public static readonly Dictionary<string, AchievementData> Data = new()
    {
        ["FIRST_KILL"] = new(
            Name: "First Blood",
            Category: AchievementCategory.Slayer,
            Description: "Defeat your first enemy."
        ),

        ["TIMBER"] = new(
            Name: "Timber!!",
            Category: AchievementCategory.Collector,
            Description: "Chop down your first tree."
        ),

        ["BENCHED"] = new(
            Name: "Benched",
            Category: AchievementCategory.Collector,
            Description: "Craft your first work bench."
        ),

        ["YOU_CAN_DO_IT"] = new(
            Name: "You Can Do It!",
            Category: AchievementCategory.Slayer,
            Description: "Survive your first full night."
        ),

        ["EYE_ON_YOU"] = new(
            Name: "Eye on You",
            Category: AchievementCategory.Challenger,
            Description: "Defeat the Eye of Cthulhu."
        ),

        ["STILL_HUNGRY"] = new(
            Name: "Still Hungry",
            Category: AchievementCategory.Challenger,
            Description: "Defeat the Wall of Flesh."
        ),

        ["CHAMPION_OF_TERRARIA"] = new(
            Name: "Champion of Terraria",
            Category: AchievementCategory.Challenger,
            Description: "Defeat the Moon Lord."
        ),
    };

    public static string CategoryName(AchievementCategory cat) => cat switch
    {
        AchievementCategory.Challenger => "Challenger",
        AchievementCategory.Collector  => "Collector",
        AchievementCategory.Explorer   => "Explorer",
        AchievementCategory.Slayer     => "Slayer",
        _ => "General",
    };

    public static AchievementData? Get(string id) =>
        Data.TryGetValue(id, out var value) ? value : null;
}
