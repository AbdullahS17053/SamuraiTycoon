using UnityEngine;
using System.Collections.Generic;

public class ZoneManager : MonoBehaviour
{
    public static ZoneManager Instance { get; private set; }

    [Header("Configuration")]
    public ZoneProgressionConfig zoneProgression;

    [Header("Current Zone")]
    public ZoneProgressionConfig.ZoneStage currentZone;
    public GameObject currentZoneInstance;

    [Header("References")]
    public Transform zoneContainer;
    public Camera mainCamera;
    public Light directionalLight;

    private GameData _data;
    private EconomyManager _economy;
    private BuildingManager3D _buildingManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(GameData data, EconomyManager economy, BuildingManager3D buildingManager)
    {
        _data = data;
        _economy = economy;
        _buildingManager = buildingManager;

        LoadCurrentZone();
        Debug.Log($"🗺️ ZoneManager initialized with zone: {currentZone.displayName}");
    }

    void LoadCurrentZone()
    {
        // Clear existing zone
        if (currentZoneInstance != null)
        {
            Destroy(currentZoneInstance);
        }

        // Get current zone
        currentZone = zoneProgression.GetCurrentZone(_data);
        if (currentZone == null)
        {
            Debug.LogError("❌ Failed to load current zone!");
            return;
        }

        // Instantiate zone prefab
        if (currentZone.zonePrefab != null)
        {
            currentZoneInstance = Instantiate(currentZone.zonePrefab, zoneContainer);
            currentZoneInstance.name = $"Zone_{currentZone.zoneId}";
        }

        // Apply zone visuals
        ApplyZoneVisuals();

        Debug.Log($"🏞️ Loaded zone: {currentZone.displayName}");
    }

    void ApplyZoneVisuals()
    {
        // Apply ground material
        if (currentZone.groundMaterial != null)
        {
            var groundRenderer = currentZoneInstance.GetComponentInChildren<Renderer>();
            if (groundRenderer != null)
            {
                groundRenderer.material = currentZone.groundMaterial;
            }
        }

        // Adjust lighting for zone atmosphere
        if (directionalLight != null)
        {
            // You can adjust lighting based on zone type
            switch (currentZone.zoneId)
            {
                case "training_ground":
                    directionalLight.color = Color.white;
                    directionalLight.intensity = 1.0f;
                    break;
                case "bamboo_forest":
                    directionalLight.color = new Color(0.9f, 1.0f, 0.8f);
                    directionalLight.intensity = 0.8f;
                    break;
                case "mountain_pass":
                    directionalLight.color = new Color(0.8f, 0.9f, 1.0f);
                    directionalLight.intensity = 0.7f;
                    break;
            }
        }

        // Play background music
        if (currentZone.backgroundMusic != null)
        {
            var audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.clip = currentZone.backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public bool CanUnlockNextZone()
    {
        var nextZone = zoneProgression.GetNextZone(currentZone.zoneId);
        return nextZone != null && zoneProgression.CanUnlockZone(nextZone.zoneId, _data, _buildingManager);
    }

    public void UnlockNextZone()
    {
        var nextZone = zoneProgression.GetNextZone(currentZone.zoneId);
        if (nextZone == null)
        {
            Debug.Log("🎉 All zones unlocked!");
            return;
        }

        if (!zoneProgression.CanUnlockZone(nextZone.zoneId, _data, _buildingManager))
        {
            Debug.LogWarning($"❌ Cannot unlock {nextZone.displayName} yet!");
            return;
        }

        // Spend gold
        if (_economy.SpendGold(nextZone.unlockCost))
        {
            // Update current zone
            _data.CurrentZoneId = nextZone.zoneId;
            currentZone = nextZone;

            // Reload zone
            LoadCurrentZone();

            // Apply zone bonuses
            ApplyZoneBonuses();

            Debug.Log($"🎊 Unlocked new zone: {nextZone.displayName}");
        }
    }

    void ApplyZoneBonuses()
    {
        // Zone bonuses are applied in income calculations
        Debug.Log($"📈 Zone bonus applied: {currentZone.incomeMultiplier}x income, {currentZone.trainingSpeedMultiplier}x training speed");
    }

    public double GetZoneIncomeMultiplier()
    {
        return currentZone?.incomeMultiplier ?? 1.0;
    }

    public float GetZoneTrainingSpeedMultiplier()
    {
        return currentZone?.trainingSpeedMultiplier ?? 1.0f;
    }

    public int GetExtraTroopCapacity()
    {
        return currentZone?.extraTroopCapacity ?? 0;
    }

    public ZoneProgressionConfig.ZoneStage GetNextAvailableZone()
    {
        return zoneProgression.GetNextZone(currentZone.zoneId);
    }

    [ContextMenu("Debug Zone Info")]
    public void DebugZoneInfo()
    {
        Debug.Log($"=== ZONE DEBUG ===");
        Debug.Log($"Current Zone: {currentZone.displayName} ({currentZone.zoneId})");
        Debug.Log($"Income Multiplier: {currentZone.incomeMultiplier}x");
        Debug.Log($"Training Speed: {currentZone.trainingSpeedMultiplier}x");

        var nextZone = GetNextAvailableZone();
        if (nextZone != null)
        {
            Debug.Log($"Next Zone: {nextZone.displayName} - Cost: {nextZone.unlockCost}");
            Debug.Log($"Can Unlock: {zoneProgression.CanUnlockZone(nextZone.zoneId, _data, _buildingManager)}");
        }
        else
        {
            Debug.Log("Next Zone: None (Max reached)");
        }
        Debug.Log($"=== END DEBUG ===");
    }
}