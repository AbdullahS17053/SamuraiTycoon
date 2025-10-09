using UnityEngine;

[CreateAssetMenu(fileName = "SpeedModule", menuName = "Samurai Tycoon/Modules/Speed")]
public class SpeedModule : BuildingModule
{
    [Header("Effect Description")]
    public string effectDescription = "Increases production speed";



    public override void OnButtonClick(TrainingBuilding building)
    {
        if (EconomyManager.Instance.SpendGold(GetCurrentCost()))
        {
            building.UpgradeEfficiency();
            BuildingPanelUI3D.Instance.OnBuildingUpgraded(null);
            level++;
        }
    }

    public override string GetStatusText(TrainingBuilding building)
    {
        throw new System.NotImplementedException();
    }

    public override string GetEffectDescription()
    {
        return effectDescription;
    }

    public float GetCurrentSpeedMultiplier()
    {
        throw new System.NotImplementedException();
    }
}