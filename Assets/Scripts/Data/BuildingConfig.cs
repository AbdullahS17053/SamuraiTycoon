using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "Samurai Tycoon/Building")]
public class BuildingConfig : ScriptableObject
{
    [Header("Basic Info")]
    public string ID;
    public string DisplayName;
    [TextArea] public string Description;

    [Header("Economics")]
    public double BaseCost = 100;
    public float CostMultiplier = 1.15f;
    public double BaseIncome = 1;
    public float IncomeMultiplier = 1.1f;

    [Header("Visuals")]
    public Sprite Icon;
    public GameObject Prefab;
    public Color ThemeColor = Color.white;

    [Header("Building Modules - DRAG MODULES HERE!")]
    [Tooltip("Add modules to create upgrade buttons in the building panel")]
    public List<BuildingModule> modules = new List<BuildingModule>();

    [Header("Module Slots")]
    [Tooltip("Maximum number of modules this building can have")]
    public int maxModuleSlots = 3;

    public Sprite Banner;

    // Helper methods
    public double GetCost(int level) => BaseCost * Mathf.Pow(CostMultiplier, level);
    public double GetIncome(int level) => BaseIncome * Mathf.Pow(IncomeMultiplier, level);

    // Editor helper methods
    public void AddModule(BuildingModule module)
    {
        if (modules.Count < maxModuleSlots && !modules.Contains(module))
        {
            modules.Add(module);
            Debug.Log($"🔧 Added {module.moduleName} to {DisplayName}");
        }
    }

    public void RemoveModule(BuildingModule module)
    {
        if (modules.Contains(module))
        {
            modules.Remove(module);
            Debug.Log($"🔧 Removed {module.moduleName} from {DisplayName}");
        }
    }
}