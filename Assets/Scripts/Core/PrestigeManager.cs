using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PrestigeManager : MonoBehaviour
{
    public static PrestigeManager Instance { get; private set; }

    [Header("Prestige UI")]
    public GameObject prestigePanel;
    public TextMeshProUGUI prestigeTitleText;
    public TextMeshProUGUI currentHonorText;
    public TextMeshProUGUI honorGainText;
    public TextMeshProUGUI prestigeBonusText;
    public TextMeshProUGUI totalPrestigesText;
    public Button prestigeButton;
    public Button closePrestigeButton;

    [Header("Prestige Rewards UI")]
    public Transform rewardsContainer;
    public GameObject rewardItemPrefab;

    [Header("Prestige Settings")]
    public double baseHonorMultiplier = 0.1f;
    public double minPrestigeThreshold = 10000;
    public float honorGrowthRate = 1.15f;

    [Header("Prestige Bonuses")]
    public PrestigeBonus[] availableBonuses;

    private GameData _data;
    private EconomyManager _economy;
    private BuildingManager3D _buildingManager;
    private TroopManager _troopManager;
    private List<PrestigeBonus> _selectedBonuses = new List<PrestigeBonus>();

    [System.Serializable]
    public class PrestigeBonus
    {
        public string bonusId;
        public string displayName;
        public string description;
        public Sprite icon;
        public BonusType type;
        public double value;
        public int requiredPrestiges = 0;

        public enum BonusType
        {
            GlobalIncomeMultiplier,
            TroopTrainingSpeed,
            BuildingCostReduction,
            OfflineEarnings,
            TroopCapacity,
            AutoTrainSpeed
        }
    }

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

    void Start()
    {
        var gameManager = FindObjectOfType<GameManager>();
        _data = gameManager?.Save.Data;
        _economy = gameManager?.Economy;
        _buildingManager = gameManager?.Buildings;
        _troopManager = FindObjectOfType<TroopManager>();

        SetupUI();
        UpdatePrestigeUI();

        Debug.Log("🔄 PrestigeManager initialized");
    }

    void SetupUI()
    {
        if (prestigeButton != null)
        {
            prestigeButton.onClick.RemoveAllListeners();
            prestigeButton.onClick.AddListener(OnPrestigeClicked);
        }

        if (closePrestigeButton != null)
        {
            closePrestigeButton.onClick.RemoveAllListeners();
            closePrestigeButton.onClick.AddListener(HidePrestigePanel);
        }
    }

    public void ShowPrestigePanel()
    {
        if (prestigePanel != null)
        {
            UpdatePrestigeUI();
            GeneratePrestigeRewards();
            prestigePanel.SetActive(true);
        }
    }

    public void HidePrestigePanel()
    {
        if (prestigePanel != null)
        {
            prestigePanel.SetActive(false);
        }
    }

    public bool CanPrestige()
    {
        if (_data == null) return false;

        double totalEarnings = CalculateTotalEarnings();
        return totalEarnings >= GetPrestigeThreshold();
    }

    public void OnPrestigeClicked()
    {
        if (!CanPrestige() || _data == null) return;

        // Show confirmation dialog
        ShowPrestigeConfirmation();
    }

    void ShowPrestigeConfirmation()
    {
        // In a real game, you'd show a proper confirmation dialog
        // For now, we'll use a simple debug log and proceed
        double honorGained = CalculateHonorGain();

        string message = $"Prestige will reset your progress but grant you {honorGained} Honor!\n\n";
        message += "You will lose:\n";
        message += "- All buildings and upgrades\n";
        message += "- All troops and their levels\n";
        message += "- Current gold and resources\n\n";
        message += "You will keep:\n";
        message += "- Honor points\n";
        message += "- Prestige bonuses\n";
        message += "- Unlocked zones\n";

        Debug.Log($"🔄 PRESTIGE CONFIRMATION:\n{message}");

        // For now, auto-confirm after 2 seconds
        Invoke("ExecutePrestige", 2f);
    }

    void ExecutePrestige()
    {
        double honorGained = CalculateHonorGain();

        // Add honor
        _data.Honor += honorGained;
        _data.TotalPrestiges++;

        // Apply selected bonuses
        ApplySelectedBonuses();

        // Reset game state but keep honor and prestige data
        ResetGameState();

        // Update UI
        UpdatePrestigeUI();

        // Hide panel
        HidePrestigePanel();

        // Refresh all managers
        RefreshAllManagers();

        Debug.Log($"🔄 Prestige #{_data.TotalPrestiges} completed! Gained {honorGained} honor");

        // Show prestige complete effect
        ShowPrestigeCompleteEffect(honorGained);
    }

    void ApplySelectedBonuses()
    {
        foreach (var bonus in _selectedBonuses)
        {
            ApplyPrestigeBonus(bonus);
        }
        _selectedBonuses.Clear();
    }

    void ApplyPrestigeBonus(PrestigeBonus bonus)
    {
        // Apply the bonus to game data
        switch (bonus.type)
        {
            case PrestigeBonus.BonusType.GlobalIncomeMultiplier:
                _data.GlobalIncomeMultiplier += bonus.value;
                break;
            case PrestigeBonus.BonusType.TroopTrainingSpeed:
                _data.TroopTrainingSpeedMultiplier += (float)bonus.value;
                break;
            case PrestigeBonus.BonusType.BuildingCostReduction:
                _data.BuildingCostReduction += bonus.value;
                break;
            case PrestigeBonus.BonusType.OfflineEarnings:
                _data.OfflineEarningsMultiplier += bonus.value;
                break;
            case PrestigeBonus.BonusType.TroopCapacity:
                _data.ExtraTroopCapacity += (int)bonus.value;
                break;
            case PrestigeBonus.BonusType.AutoTrainSpeed:
                _data.AutoTrainSpeedMultiplier += (float)bonus.value;
                break;
        }

        Debug.Log($"🎁 Applied prestige bonus: {bonus.displayName}");
    }

    void ResetGameState()
    {
        if (_data == null) return;

        // Reset currencies (keep honor)
        _data.Gold = GetPrestigeStartingGold();
        _data.Samurai = 0;
        _data.Peasants = 0;

        // Reset buildings but keep them unlocked at level 1
        foreach (var buildingData in _data.Buildings)
        {
            buildingData.Level = 1;
            buildingData.IsUnlocked = true; // Keep buildings unlocked after first prestige
        }

        // Clear troops
        if (_troopManager != null)
        {
            // This would need to be implemented in TroopManager
            _troopManager.ResetTroops();
        }

        // Reset session-specific data
        _data.LastSaveTime = System.DateTime.Now;

        // Save game
        FindObjectOfType<GameManager>()?.Save.SaveGame();
    }

    void RefreshAllManagers()
    {
        // Refresh building manager
        if (_buildingManager != null)
        {
            _buildingManager.Initialize(_data, _economy);
        }

        // Refresh troop manager
        if (_troopManager != null)
        {
            _troopManager.InitializeTroops();
        }

        // Refresh UI
        var uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.Initialize(_data, _economy, _buildingManager);
        }
    }

    void GeneratePrestigeRewards()
    {
        // Clear existing rewards
        foreach (Transform child in rewardsContainer)
        {
            Destroy(child.gameObject);
        }

        _selectedBonuses.Clear();

        // Get available bonuses for current prestige level
        var availableRewards = GetAvailableBonuses();

        // Select 3 random bonuses
        for (int i = 0; i < Mathf.Min(3, availableRewards.Count); i++)
        {
            var bonus = availableRewards[Random.Range(0, availableRewards.Count)];
            availableRewards.Remove(bonus);

            CreateRewardItem(bonus);
        }
    }

    void CreateRewardItem(PrestigeBonus bonus)
    {
        if (rewardItemPrefab == null) return;

        var rewardObj = Instantiate(rewardItemPrefab, rewardsContainer);
        var rewardUI = rewardObj.GetComponent<PrestigeRewardUI>();

        if (rewardUI != null)
        {
            rewardUI.Initialize(bonus, this);
        }
    }

    public void SelectBonus(PrestigeBonus bonus)
    {
        if (!_selectedBonuses.Contains(bonus))
        {
            _selectedBonuses.Add(bonus);
            UpdatePrestigeUI();
        }
    }

    public void DeselectBonus(PrestigeBonus bonus)
    {
        _selectedBonuses.Remove(bonus);
        UpdatePrestigeUI();
    }

    List<PrestigeBonus> GetAvailableBonuses()
    {
        List<PrestigeBonus> available = new List<PrestigeBonus>();

        foreach (var bonus in availableBonuses)
        {
            if (_data.TotalPrestiges >= bonus.requiredPrestiges)
            {
                available.Add(bonus);
            }
        }

        return available;
    }

    double CalculateTotalEarnings()
    {
        // Calculate based on buildings, troops, and achievements
        double total = 0;

        if (_buildingManager != null && _data != null)
        {
            // Building value
            foreach (var buildingData in _data.Buildings)
            {
                if (buildingData.IsUnlocked)
                {
                    total += buildingData.Level * 1000;
                }
            }
        }

        // Troop value
        if (_troopManager != null)
        {
            total += _troopManager.GetTotalTroops() * 500;
        }

        return total;
    }

    double CalculateHonorGain()
    {
        double baseHonor = CalculateTotalEarnings() * baseHonorMultiplier;
        double prestigeBonus = Mathf.Pow(honorGrowthRate, _data.TotalPrestiges);
        return System.Math.Floor(baseHonor * prestigeBonus);
    }

    double GetPrestigeThreshold()
    {
        return minPrestigeThreshold * Mathf.Pow(1.5f, _data.TotalPrestiges);
    }

    double GetPrestigeStartingGold()
    {
        // Start with more gold on higher prestiges
        return 1000 * Mathf.Pow(2, _data.TotalPrestiges);
    }

    void UpdatePrestigeUI()
    {
        if (_data == null) return;

        double honorGain = CalculateHonorGain();
        double threshold = GetPrestigeThreshold();
        bool canPrestige = CanPrestige();

        // Update texts
        if (prestigeTitleText != null)
        {
            prestigeTitleText.text = $"Prestige #{_data.TotalPrestiges + 1}";
        }

        if (currentHonorText != null)
        {
            currentHonorText.text = $"Honor: {_data.Honor}";
        }

        if (honorGainText != null)
        {
            honorGainText.text = canPrestige ?
                $"Prestige Reward: +{honorGain} Honor" :
                $"Need {threshold:F0} total earnings to prestige";
        }

        if (prestigeBonusText != null)
        {
            double incomeBonus = GetTotalIncomeBonus() * 100;
            prestigeBonusText.text = $"Total Bonus: +{incomeBonus:F1}% Income";
        }

        if (totalPrestigesText != null)
        {
            totalPrestigesText.text = $"Total Prestiges: {_data.TotalPrestiges}";
        }

        if (prestigeButton != null)
        {
            prestigeButton.interactable = canPrestige && _selectedBonuses.Count > 0;
            prestigeButton.GetComponentInChildren<TextMeshProUGUI>().text =
                canPrestige ? $"PRESTIGE (+{honorGain} Honor)" : "NOT READY";
        }
    }

    double GetTotalIncomeBonus()
    {
        return _data.GlobalIncomeMultiplier;
    }

    void ShowPrestigeCompleteEffect(double honorGained)
    {
        // Play sound
        AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("PrestigeSound"), Camera.main.transform.position);

        // Show visual effect
        GameObject effect = Instantiate(Resources.Load<GameObject>("PrestigeEffect"), Vector3.zero, Quaternion.identity);
        Destroy(effect, 3f);

        // Show honor gain popup
        Debug.Log($"🎉 PRESTIGE COMPLETE! Gained {honorGained} Honor!");
    }

    void Update()
    {
        // Update UI every second
        if (Time.time % 1.0f < Time.deltaTime)
        {
            UpdatePrestigeUI();
        }
    }

    [ContextMenu("Force Prestige")]
    public void ForcePrestige()
    {
        ExecutePrestige();
    }

    [ContextMenu("Debug Prestige Info")]
    public void DebugPrestigeInfo()
    {
        Debug.Log($"=== PRESTIGE DEBUG ===");
        Debug.Log($"Total Prestiges: {_data.TotalPrestiges}");
        Debug.Log($"Current Honor: {_data.Honor}");
        Debug.Log($"Total Earnings: {CalculateTotalEarnings()}");
        Debug.Log($"Prestige Threshold: {GetPrestigeThreshold()}");
        Debug.Log($"Can Prestige: {CanPrestige()}");
        Debug.Log($"Honor Gain: {CalculateHonorGain()}");
        Debug.Log($"Selected Bonuses: {_selectedBonuses.Count}");
        Debug.Log($"=== END DEBUG ===");
    }
}