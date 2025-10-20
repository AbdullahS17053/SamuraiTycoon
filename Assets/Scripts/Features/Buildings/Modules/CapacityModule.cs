using UnityEngine;

[CreateAssetMenu(fileName = "CapacityModule", menuName = "Samurai Tycoon/Modules/Capacity")]
public class CapacityModule : BuildingModule
{
    [Header("Capacity Settings")]
    public int baseCapacity = 1;
    public int maxCapacity = 10;

    [Header("Effect Description")]
    public string effectDescription = "Increases workforce capacity";


    public override void OnButtonClick(TrainingBuilding building)
    {
        if (EconomyManager.Instance.SpendGold(GetCurrentCost()))
        {
            building.UpgradeCapacity();
            BuildingPanelUI3D.Instance.OnBuildingUpgraded(null);
            level++;
            AdManager.instance.OpenAd();
        }
    }

    public override string GetEffectDescription()
    {
        return effectDescription;
    }

    // Capacity module reaches max level when all worker slots are filled
    public override bool IsMaxLevel()
    {
        throw new System.NotImplementedException();
    }

    public override string GetStatusText(TrainingBuilding building)
    {
        throw new System.NotImplementedException();
    }
}