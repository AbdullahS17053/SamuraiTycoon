using UnityEngine;

[CreateAssetMenu(fileName = "IncomeModule", menuName = "Samurai Tycoon/Modules/Income")]
public class IncomeModule : BuildingModule
{
    [Header("Effect Description")]
    public string effectDescription = "Increases building income";



    public override void OnButtonClick(TrainingBuilding building)
    {
        if (EconomyManager.Instance.SpendGold(GetCurrentCost()))
        {
            building.UpgradeIncome();
            BuildingPanelUI3D.Instance.OnBuildingUpgraded(null);
            level++;
            AdManager.instance.OpenAd();
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

    public override bool IsValid()
    {
        throw new System.NotImplementedException();
    }
}