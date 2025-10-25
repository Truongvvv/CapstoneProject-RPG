    using System;
    using System.IO;
    using UnityEngine;

    public class DataManager
    {
        private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");

        public static PlayerData CurrentData { get; set; } = new PlayerData();

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
                CurrentData = new PlayerData();
                return;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                CurrentData = JsonUtility.FromJson<PlayerData>(json);
                Debug.Log("✅ Game Loaded Successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Load failed: {e.Message}");
                CurrentData = new PlayerData();
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

        #region === EXTENDED SAVE HELPERS ===
        
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

        public static void UpdateGold(int gold)
        {
            CurrentData.gold = gold;
        }

        public static void UpdateInventory(System.Collections.Generic.List<string> items)
        {
            CurrentData.itemsCollected = new(items);
        }

        public static void UpdateBossDefeated(string bossId)
        {
            if (!CurrentData.bossesDefeated.Contains(bossId))
                CurrentData.bossesDefeated.Add(bossId);
        }

        public static void UpdateQuest(string questId, bool completed)
        {
            if (completed && !CurrentData.completedQuests.Contains(questId))
                CurrentData.completedQuests.Add(questId);
        }

        public static void UpdateSkillCooldown(string skillName, float cooldown)
        {
            if (CurrentData.skillCooldowns.ContainsKey(skillName))
                CurrentData.skillCooldowns[skillName] = cooldown;
            else
                CurrentData.skillCooldowns.Add(skillName, cooldown);
        }

        public static void UpdateSettings(float musicVol, float sfxVol)
        {
            CurrentData.musicVolume = musicVol;
            CurrentData.sfxVolume = sfxVol;
        }

        #endregion
    }

    /// <summary>
    /// Dữ liệu chính lưu vào file JSON
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        // === PLAYER STATS ===
        public int playerLevel = 1;
        public int playerExp = 0;
        public int playerMaxHP = 100;
        public int playerCurrentHP = 100;
        public float playerDamage = 10f;
        public float moveSpeed = 5f;
        public int gold = 0;

        // === POSITION ===
        public float positionX;
        public float positionY;
        public float positionZ;

        // === QUEST / BOSS / INVENTORY ===
        public System.Collections.Generic.List<string> completedQuests = new();
        public System.Collections.Generic.List<string> bossesDefeated = new();
        public System.Collections.Generic.List<string> itemsCollected = new();

        // === SKILL DATA ===
        public System.Collections.Generic.Dictionary<string, float> skillCooldowns = new();

        // === SETTINGS ===
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public float playTime = 0f;

        public PlayerData() { }
    }

