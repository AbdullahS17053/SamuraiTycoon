using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PrestigeRewardUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject prestigePanel;
    public Transform rewardsContainer;
    public GameObject rewardItemPrefab;
    public TextMeshProUGUI honorText;
    public TextMeshProUGUI prestigeCountText;
    public Button prestigeButton;
    public Button closeButton;

    [Header("Reward Display")]
    public Image selectedRewardIcon;
    public TextMeshProUGUI selectedRewardName;
    public TextMeshProUGUI selectedRewardDescription;
    public TextMeshProUGUI selectedRewardCost;
    public Button purchaseButton;
    public TextMeshProUGUI purchaseButtonText;

    [Header("Prestige Info")]
    public TextMeshProUGUI prestigeRewardPreview;
    public TextMeshProUGUI prestigeRequirements;

    private PrestigeManager _prestigeManager;
    private List<GameObject> _rewardItems = new List<GameObject>();
    private PrestigeManager.PrestigeBonus _selectedBonus;

    void Start()
    {
        _prestigeManager = FindObjectOfType<PrestigeManager>();

        if (_prestigeManager == null)
        {
            Debug.LogError("❌ PrestigeManager not found!");
            return;
        }

        SetupUI();
        UpdateUI();
    }

    void SetupUI()
    {
        // Setup button listeners
        if (prestigeButton != null)
        {
            prestigeButton.onClick.RemoveAllListeners();
            prestigeButton.onClick.AddListener(OnPrestigeClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(OnPurchaseClicked);
        }
    }

    public void ShowPrestigePanel()
    {
        if (prestigePanel != null)
        {
            prestigePanel.SetActive(true);
            UpdateUI();
            Debug.Log("📊 Prestige panel shown");
        }
    }

    public void HidePrestigePanel()
    {
        if (prestigePanel != null)
        {
            prestigePanel.SetActive(false);
            Debug.Log("📊 Prestige panel hidden");
        }
    }

    void UpdateUI()
    {
        UpdateHonorDisplay();
        UpdatePrestigeCount();
        UpdateRewardsList();
        UpdateSelectedReward();
        UpdatePrestigePreview();
    }

    void UpdateHonorDisplay()
    {
        if (honorText != null && _prestigeManager != null)
        {
            honorText.text = $"Available Honor: {_prestigeManager.availableHonor:F0}";
        }
    }

    void UpdatePrestigeCount()
    {
        if (prestigeCountText != null && _prestigeManager != null)
        {
            prestigeCountText.text = $"Prestige Count: {_prestigeManager.prestigeCount}";
        }
    }

    void UpdatePrestigePreview()
    {
        if (_prestigeManager != null)
        {
            if (prestigeRewardPreview != null)
            {
                double reward = _prestigeManager.GetHonorRewardPreview();
                prestigeRewardPreview.text = $"Next Prestige: {reward:F0} Honor";
            }

            if (prestigeRequirements != null)
            {
                bool canPrestige = _prestigeManager.CanPrestige();
                prestigeRequirements.text = canPrestige ?
                    "Ready to Prestige!" :
                    $"Need {_prestigeManager.minimumPrestigeGold:F0} Gold to Prestige";

                prestigeRequirements.color = canPrestige ? Color.green : Color.yellow;
            }

            if (prestigeButton != null)
            {
                prestigeButton.interactable = _prestigeManager.CanPrestige();
            }
        }
    }

    void UpdateRewardsList()
    {
        // Clear existing reward items
        foreach (var item in _rewardItems)
        {
            if (item != null)
                Destroy(item);
        }
        _rewardItems.Clear();

        if (_prestigeManager == null || _prestigeManager.availableBonuses == null)
            return;

        // Create reward items
        foreach (var bonus in _prestigeManager.availableBonuses)
        {
            if (bonus != null)
            {
                CreateRewardItem(bonus);
            }
        }
    }

    void CreateRewardItem(PrestigeManager.PrestigeBonus bonus)
    {
        if (rewardItemPrefab == null || rewardsContainer == null)
            return;

        GameObject rewardItem = Instantiate(rewardItemPrefab, rewardsContainer);
        var rewardUI = rewardItem.GetComponent<PrestigeRewardItemUI>();

        if (rewardUI != null)
        {
            rewardUI.Initialize(
                bonus.displayName ?? bonus.bonusName,
                bonus.description,
                bonus.honorCost,
                bonus.isPurchased,
                () => OnRewardSelected(bonus)
            );

            // Update affordability
            rewardUI.UpdateWithCurrentHonor(_prestigeManager.availableHonor);
        }
        else
        {
            Debug.LogError("❌ PrestigeRewardItemUI component not found on reward item prefab!");
        }

        _rewardItems.Add(rewardItem);
    }

    void OnRewardSelected(PrestigeManager.PrestigeBonus bonus)
    {
        _selectedBonus = bonus;

        if (_prestigeManager != null)
        {
            _prestigeManager.SelectBonus(bonus);
        }

        // Update all reward items selection state
        UpdateAllRewardItemsSelection();

        UpdateSelectedReward();
        Debug.Log($"🎯 Selected reward: {bonus.displayName ?? bonus.bonusName}");
    }

    void UpdateAllRewardItemsSelection()
    {
        foreach (var item in _rewardItems)
        {
            if (item != null)
            {
                var rewardUI = item.GetComponent<PrestigeRewardItemUI>();
                if (rewardUI != null)
                {
                    bool isSelected = (_selectedBonus != null &&
                                      rewardUI.rewardNameText != null &&
                                      rewardUI.rewardNameText.text == (_selectedBonus.displayName ?? _selectedBonus.bonusName));
                    rewardUI.SetSelectedState(isSelected);
                }
            }
        }
    }

    void UpdateSelectedReward()
    {
        if (_selectedBonus == null)
        {
            // No reward selected
            if (selectedRewardIcon != null)
                selectedRewardIcon.gameObject.SetActive(false);

            if (selectedRewardName != null)
                selectedRewardName.text = "Select a Reward";

            if (selectedRewardDescription != null)
                selectedRewardDescription.text = "Choose a prestige bonus from the list";

            if (selectedRewardCost != null)
                selectedRewardCost.text = "";

            if (purchaseButton != null)
                purchaseButton.interactable = false;

            if (purchaseButtonText != null)
                purchaseButtonText.text = "Select a Reward First";

            return;
        }

        // Update selected reward display
        if (selectedRewardIcon != null)
        {
            selectedRewardIcon.gameObject.SetActive(_selectedBonus.icon != null);
            if (_selectedBonus.icon != null)
            {
                selectedRewardIcon.sprite = _selectedBonus.icon;
            }
        }

        if (selectedRewardName != null)
            selectedRewardName.text = _selectedBonus.displayName ?? _selectedBonus.bonusName;

        if (selectedRewardDescription != null)
            selectedRewardDescription.text = _selectedBonus.description;

        if (selectedRewardCost != null)
            selectedRewardCost.text = $"Cost: {_selectedBonus.honorCost} Honor";

        if (purchaseButton != null && purchaseButtonText != null)
        {
            bool canPurchase = _prestigeManager != null &&
                              _prestigeManager.CanPurchaseBonus(_selectedBonus);

            purchaseButton.interactable = canPurchase && !_selectedBonus.isPurchased;

            if (_selectedBonus.isPurchased)
            {
                purchaseButtonText.text = "Already Purchased";
                purchaseButton.interactable = false;
            }
            else if (canPurchase)
            {
                purchaseButtonText.text = "Purchase";
            }
            else
            {
                purchaseButtonText.text = "Not Enough Honor";
            }
        }
    }

    void OnPurchaseClicked()
    {
        if (_selectedBonus != null && _prestigeManager != null)
        {
            bool purchased = _prestigeManager.PurchaseBonus(_selectedBonus);
            if (purchased)
            {
                Debug.Log($"✅ Purchased: {_selectedBonus.displayName ?? _selectedBonus.bonusName}");

                // Update all UI elements
                UpdateUI();

                // Refresh reward items to show purchased state
                UpdateRewardsList();

                // Clear selection
                _selectedBonus = null;
                UpdateSelectedReward();
            }
            else
            {
                Debug.Log($"❌ Failed to purchase: {_selectedBonus.displayName ?? _selectedBonus.bonusName}");
            }
        }
    }

    void OnPrestigeClicked()
    {
        if (_prestigeManager != null)
        {
            if (_prestigeManager.CanPrestige())
            {
                _prestigeManager.PrestigeReset();
                UpdateUI();
                Debug.Log("👑 Prestige completed!");
            }
            else
            {
                Debug.Log("❌ Not enough gold to prestige!");
                // You could show a popup message to the player here
            }
        }
    }

    void OnCloseClicked()
    {
        HidePrestigePanel();
    }

    void Update()
    {
        // Update UI periodically when panel is open
        if (prestigePanel != null && prestigePanel.activeInHierarchy)
        {
            if (Time.frameCount % 30 == 0) // Update every 30 frames
            {
                UpdateHonorDisplay();
                UpdatePrestigePreview();

                // Update affordability of all reward items
                if (_prestigeManager != null)
                {
                    foreach (var item in _rewardItems)
                    {
                        if (item != null)
                        {
                            var rewardUI = item.GetComponent<PrestigeRewardItemUI>();
                            if (rewardUI != null && !rewardUI.IsPurchased())
                            {
                                rewardUI.UpdateWithCurrentHonor(_prestigeManager.availableHonor);
                            }
                        }
                    }
                }
            }
        }
    }

    [ContextMenu("Debug Prestige UI")]
    public void DebugPrestigeUI()
    {
        Debug.Log("=== PRESTIGE UI DEBUG ===");
        Debug.Log($"Prestige Manager: {_prestigeManager != null}");
        Debug.Log($"Selected Bonus: {_selectedBonus?.displayName ?? _selectedBonus?.bonusName ?? "None"}");
        Debug.Log($"Reward Items: {_rewardItems.Count}");

        if (_prestigeManager != null)
        {
            Debug.Log($"Available Bonuses: {_prestigeManager.availableBonuses?.Count ?? 0}");
            Debug.Log($"Available Honor: {_prestigeManager.availableHonor}");
            Debug.Log($"Can Prestige: {_prestigeManager.CanPrestige()}");
        }
    }

    [ContextMenu("Test Add Honor")]
    public void TestAddHonor()
    {
        if (_prestigeManager != null)
        {
            _prestigeManager.availableHonor += 100;
            UpdateUI();
            Debug.Log("➕ Added 100 test honor");
        }
    }
}