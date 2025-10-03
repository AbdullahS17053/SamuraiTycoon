using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ZoneProgression", menuName = "Samurai Tycoon/Zone Progression")]
public class ZoneProgressionConfig : ScriptableObject
{
    [System.Serializable]
    public class ZoneStage
    {
        [Header("Zone Identification")]
        public string zoneId;
        public string displayName;
        public string description;
        public Sprite zoneIcon;
        public Color zoneColor = Color.white;

        [Header("Unlock Requirements")]
        public double unlockCost;
        public int requiredPrestigeLevel = 0;
        public string requiredBuildingId;
        public int requiredBuildingLevel = 10;

        [Header("Zone Bonuses")]
        public double incomeMultiplier = 1.0;
        public float trainingSpeedMultiplier = 1.0f;
        public int extraTroopCapacity = 0;

        [Header("Visuals")]
        public GameObject zonePrefab;
        public Material groundMaterial;
        public AudioClip backgroundMusic;
    }

    [Header("Zone Progression Order")]
    public List<ZoneStage> zones = new List<ZoneStage>();

    [Header("Default Zone")]
    public string startingZoneId = "training_ground";

    public ZoneStage GetZone(string zoneId)
    {
        return zones.Find(z => z.zoneId == zoneId);
    }

    public ZoneStage GetNextZone(string currentZoneId)
    {
        int currentIndex = zones.FindIndex(z => z.zoneId == currentZoneId);
        return currentIndex >= 0 && currentIndex < zones.Count - 1 ? zones[currentIndex + 1] : null;
    }

    public bool CanUnlockZone(string zoneId, GameData data, BuildingManager3D buildingManager)
    {
        var zone = GetZone(zoneId);
        if (zone == null) return false;

        // Check prestige requirement
        if (data.TotalPrestiges < zone.requiredPrestigeLevel)
            return false;

        // Check building requirement
        if (!string.IsNullOrEmpty(zone.requiredBuildingId))
        {
            var buildingData = data.Buildings.Find(b => b.ID == zone.requiredBuildingId);
            if (buildingData == null || buildingData.Level < zone.requiredBuildingLevel)
                return false;
        }

        // Check cost
        return data.Gold >= zone.unlockCost;
    }

    public ZoneStage GetCurrentZone(GameData data)
    {
        if (string.IsNullOrEmpty(data.CurrentZoneId))
        {
            data.CurrentZoneId = startingZoneId;
        }
        return GetZone(data.CurrentZoneId);
    }
}