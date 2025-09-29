using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "SpeedModule", menuName = "Samurai Tycoon/Modules/Speed")]
public class SpeedModule : BuildingModule
{
    [Header("Speed Settings")]
    public float baseSpeed = 1.0f;
    public float speedMultiplier = 1.05f;
    public float maxSpeed = 5.0f;

    [Header("Speed Boost")]
    public float boostDuration = 30f;
    public float boostMultiplier = 2f;
    private double boostTimer;
    private bool isBoosted;

    public override void Initialize(BuildingModuleData data)
    {
        runtimeData = data;
        Debug.Log($"⚡ SpeedModule initialized");
    }

    public override void OnBuildingTick(Building building, double deltaTime)
    {
        if (isBoosted)
        {
            boostTimer -= deltaTime;
            if (boostTimer <= 0)
            {
                isBoosted = false;
                Debug.Log($"📉 Speed boost ended for {building.Config.DisplayName}");
            }
        }
    }

    public override void OnUpgrade(Building building, int oldLevel, int newLevel)
    {
        Debug.Log($"⚡ Speed increased: {GetCurrentSpeed(building):F2}x");
    }

    public override void OnButtonClick(Building building)
    {
        // Activate speed boost
        if (!isBoosted)
        {
            isBoosted = true;
            boostTimer = boostDuration;
            Debug.Log($"🚀 Speed boost activated for {building.Config.DisplayName} ({boostDuration}s)");
            TriggerStarted(building.Data.ID);
        }
    }

    public override string GetStatusText(Building building)
    {
        float speed = GetCurrentSpeed(building);
        string boostStatus = isBoosted ? $" (BOOSTED! {boostTimer:F0}s)" : "";
        return $"Speed: {speed:F2}x{boostStatus}";
    }

    public float GetCurrentSpeed(Building building)
    {
        float speed = baseSpeed * Mathf.Pow(speedMultiplier, building.Data.Level);
        speed = Mathf.Min(speed, maxSpeed);
        return isBoosted ? speed * boostMultiplier : speed;
    }
}