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

    [Header("Modules - DRAG MODULES HERE!")]
    public List<BuildingModule> modules = new List<BuildingModule>();

    // Helper methods
    public double GetCost(int level) => BaseCost * Mathf.Pow(CostMultiplier, level);
    public double GetIncome(int level) => BaseIncome * Mathf.Pow(IncomeMultiplier, level);
}