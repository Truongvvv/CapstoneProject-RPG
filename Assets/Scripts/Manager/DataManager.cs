using System;
using System.IO;
using UnityEngine;

public class DataManager
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");

    public static GameData CurrentData { get; private set; } = new GameData();

    #region === SAVE / LOAD ===

    public static void SaveGame()
    {
        try
        {
            string json = JsonUtility.ToJson(CurrentData, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"💾 Game Saved! Path: {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Save failed: {e.Message}");
        }
    }

    public static void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("⚠ No save file found. Creating new data...");
            CurrentData = new GameData();
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            CurrentData = JsonUtility.FromJson<GameData>(json);
            Debug.Log("✅ Game Loaded Successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Load failed: {e.Message}");
            CurrentData = new GameData();
        }
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("🗑 Save file deleted.");
        }
    }

    #endregion

    #region === EXAMPLES: Update Methods ===

    public static void UpdateHealth(int current, int max)
    {
        CurrentData.playerCurrentHP = current;
        CurrentData.playerMaxHP = max;
    }

    public static void UpdateExp(int exp, int level)
    {
        CurrentData.playerExp = exp;
        CurrentData.playerLevel = level;
    }

    public static void UpdatePosition(Vector3 pos)
    {
        CurrentData.positionX = pos.x;
        CurrentData.positionY = pos.y;
        CurrentData.positionZ = pos.z;
    }

    public static void MarkQuestCompleted(string questId)
    {
        if (!CurrentData.completedQuests.Contains(questId))
            CurrentData.completedQuests.Add(questId);
    }

    #endregion
}

/// <summary>
/// Dữ liệu chính lưu vào file JSON
/// </summary>
[Serializable]
public class GameData
{
    public int playerLevel = 1;
    public int playerExp = 0;
    public int playerMaxHP = 100;
    public int playerCurrentHP = 100;
    public float playerDamage = 10f;
    public int gold = 0;

    public float positionX;
    public float positionY;
    public float positionZ;

    public System.Collections.Generic.List<string> completedQuests = new();

    public float playTime = 0f;

    public System.Collections.Generic.List<string> bossesDefeated = new();

    public System.Collections.Generic.List<string> itemsCollected = new();

    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    public GameData() { }
}
