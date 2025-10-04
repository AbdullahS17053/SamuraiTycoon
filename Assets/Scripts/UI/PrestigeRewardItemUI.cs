using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrestigeRewardItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI rewardNameText;
    public TextMeshProUGUI rewardDescriptionText;
    public TextMeshProUGUI rewardCostText;
    public Button selectButton;
    public Image backgroundImage;
    public GameObject purchasedOverlay;
    public Image rewardIcon;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color purchasedColor = Color.green;
    public Color selectedColor = Color.yellow;
    public Color affordableColor = Color.blue;
    public Color expensiveColor = Color.red;

    private System.Action _onSelectCallback;
    private bool _isPurchased = false;
    private double _cost = 0;

    public void Initialize(string name, string description, double cost, bool isPurchased, System.Action onSelect)
    {
        // Set text values
        if (rewardNameText != null)
            rewardNameText.text = name ?? "Unknown Reward";

        if (rewardDescriptionText != null)
            rewardDescriptionText.text = description ?? "No description available";

        if (rewardCostText != null)
            rewardCostText.text = $"{cost:F0} Honor";

        // Store cost and purchase state
        _cost = cost;
        _isPurchased = isPurchased;

        // Set purchase state
        SetPurchasedState(isPurchased);

        // Set callback
        _onSelectCallback = onSelect;

        // Setup button
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            if (!isPurchased)
            {
                selectButton.onClick.AddListener(OnButtonClicked);
                selectButton.interactable = true;
            }
            else
            {
                selectButton.interactable = false;
            }
        }

        // Update visual state
        UpdateVisualState();
    }

    public void SetPurchasedState(bool purchased)
    {
        _isPurchased = purchased;

        if (purchasedOverlay != null)
            purchasedOverlay.SetActive(purchased);

        if (selectButton != null)
            selectButton.interactable = !purchased;

        UpdateVisualState();
    }

    public void SetSelectedState(bool selected)
    {
        if (backgroundImage != null && !_isPurchased)
        {
            backgroundImage.color = selected ? selectedColor : normalColor;
        }
        else if (backgroundImage != null && _isPurchased)
        {
            backgroundImage.color = purchasedColor;
        }
    }

    public void SetAffordability(bool canAfford)
    {
        if (rewardCostText != null && !_isPurchased)
        {
            rewardCostText.color = canAfford ? affordableColor : expensiveColor;
        }
    }

    private void UpdateVisualState()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = _isPurchased ? purchasedColor : normalColor;
        }

        if (rewardCostText != null)
        {
            if (_isPurchased)
            {
                rewardCostText.text = "PURCHASED";
                rewardCostText.color = purchasedColor;
            }
            else
            {
                rewardCostText.text = $"{_cost:F0} Honor";
            }
        }
    }

    private void OnButtonClicked()
    {
        _onSelectCallback?.Invoke();
    }

    // Helper method to check if purchased
    public bool IsPurchased()
    {
        return _isPurchased;
    }

    // Helper method to get cost
    public double GetCost()
    {
        return _cost;
    }

    // Method to update with current honor (for affordability check)
    public void UpdateWithCurrentHonor(double currentHonor)
    {
        if (!_isPurchased)
        {
            SetAffordability(currentHonor >= _cost);
        }
    }

    [ContextMenu("Debug Reward Item")]
    public void DebugRewardItem()
    {
        Debug.Log($"🎁 Reward Item: {rewardNameText?.text}, Cost: {_cost}, Purchased: {_isPurchased}");
    }
}