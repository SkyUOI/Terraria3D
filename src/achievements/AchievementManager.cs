using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;

namespace Terraria3D.achievements;

/// <summary>
/// Global per-game-install achievement tracker. Achievements are not
/// tied to a character or world — once unlocked, they stay unlocked
/// forever. Persisted to <c>user://achievements.json</c>.
/// </summary>
public static class AchievementManager
{
    private const string SavePath = "user://achievements.json";

    private static readonly HashSet<string> _unlocked = [];

    /// <summary>Read-only set of unlocked achievement IDs.</summary>
    public static IReadOnlySet<string> Unlocked => _unlocked;

    /// <summary>Number of achievements unlocked.</summary>
    public static int UnlockedCount => _unlocked.Count;

    /// <summary>Total number of achievements in the registry.</summary>
    public static int TotalCount => AchievementRegistry.Data.Count;

    // ── Unlock ──────────────────────────────────────────────────────────

    /// <summary>
    /// Unlock an achievement by ID. Does nothing if already unlocked or
    /// the ID is not in the registry. Returns true if newly unlocked.
    /// </summary>
    public static bool Unlock(string id)
    {
        if (!AchievementRegistry.Data.ContainsKey(id)) return false;
        if (!_unlocked.Add(id)) return false;

        Save();
        GD.Print($"[Achievement] Unlocked: {AchievementRegistry.Data[id].Name}");
        return true;
    }

    /// <summary>Check if an achievement is unlocked.</summary>
    public static bool IsUnlocked(string id) => _unlocked.Contains(id);

    // ── Query ───────────────────────────────────────────────────────────

    /// <summary>
    /// Get all achievements grouped by category for UI display.
    /// Each entry includes the ID, data, and whether it's unlocked.
    /// </summary>
    public static Dictionary<AchievementCategory, List<(string Id, AchievementData Data, bool Unlocked)>> GetAllGrouped()
    {
        var grouped = new Dictionary<AchievementCategory, List<(string, AchievementData, bool)>>();

        foreach (var (id, data) in AchievementRegistry.Data)
        {
            if (!grouped.ContainsKey(data.Category))
                grouped[data.Category] = [];

            grouped[data.Category].Add((id, data, IsUnlocked(id)));
        }

        return grouped;
    }

    // ── Persistence ─────────────────────────────────────────────────────

    /// <summary>Load unlocked achievements from disk.</summary>
    public static void Load()
    {
        var path = ProjectSettings.GlobalizePath(SavePath);
        if (!File.Exists(path)) return;

        try
        {
            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<SaveData>(json);
            if (data?.Unlocked != null)
            {
                _unlocked.Clear();
                foreach (var id in data.Unlocked)
                {
                    if (AchievementRegistry.Data.ContainsKey(id))
                        _unlocked.Add(id);
                }
            }
            GD.Print($"[Achievement] Loaded {_unlocked.Count} unlocked achievements");
        }
        catch (Exception ex)
        {
            GD.PushError($"[Achievement] Failed to load: {ex.Message}");
        }
    }

    /// <summary>Save unlocked achievements to disk.</summary>
    public static void Save()
    {
        try
        {
            var data = new SaveData { Unlocked = _unlocked.ToArray() };
            var path = ProjectSettings.GlobalizePath(SavePath);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(path, JsonSerializer.Serialize(data));
        }
        catch (Exception ex)
        {
            GD.PushError($"[Achievement] Failed to save: {ex.Message}");
        }
    }

    [Serializable]
    private class SaveData
    {
        public string[] Unlocked { get; set; } = [];
    }
}
