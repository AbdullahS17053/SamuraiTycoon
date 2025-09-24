using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "Samurai Tycoon/Building")]
public class BuildingConfig : ScriptableObject
{
    [Header("Basic Info")]
    public string ID;                      // Unique ID (must match BuildingData.ID)
    public string DisplayName;             // Name shown in game
    [TextArea] public string Description;  // Building description

    [Header("Economics")]
    public double BaseCost = 100;          // Cost at level 1
    public float CostMultiplier = 1.15f;   // How much cost increases per level
    public double BaseIncome = 1;          // Gold per second at level 1
    public float IncomeMultiplier = 1.1f;  // Income growth per level

    [Header("Visuals")]
    public Sprite Icon;                    // UI icon
    public GameObject Prefab;              // 3D model (optional)
    public Color ThemeColor = Color.white; // UI accent color

    // Helper methods - USED AUTOMATICALLY BY SYSTEM
    public double GetCost(int level) => BaseCost * Mathf.Pow(CostMultiplier, level);
    public double GetIncome(int level) => BaseIncome * Mathf.Pow(IncomeMultiplier, level);
}