using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NFTCommanderUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI commanderNameText;
    public TextMeshProUGUI commanderRarityText;
    public TextMeshProUGUI commanderStatsText;
    public Image commanderImage;
    public Button equipButton;
    public GameObject equippedIndicator;

    [Header("Rarity Colors")]
    public Color commonColor = Color.white;
    public Color uncommonColor = Color.green;
    public Color rareColor = Color.blue;
    public Color epicColor = Color.magenta;
    public Color legendaryColor = Color.yellow;
    public Color mythicColor = Color.red;

    private Web3Manager.NFTCommander _commander;
    private Web3Manager _web3Manager;

    public void Initialize(Web3Manager.NFTCommander commander, Web3Manager manager)
    {
        _commander = commander;
        _web3Manager = manager;

        UpdateUI();

        if (equipButton != null)
        {
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(OnEquipClicked);
        }
    }

    void UpdateUI()
    {
        if (commanderNameText != null)
            commanderNameText.text = _commander.name;

        if (commanderRarityText != null)
        {
            commanderRarityText.text = _commander.rarity.ToString().ToUpper();
            commanderRarityText.color = GetRarityColor(_commander.rarity);
        }

        if (commanderStatsText != null)
            commanderStatsText.text = GetCommanderStats();

        if (equippedIndicator != null)
            equippedIndicator.SetActive(_commander.isEquipped);

        if (equipButton != null)
        {
            equipButton.interactable = !_commander.isEquipped;
            equipButton.GetComponentInChildren<TextMeshProUGUI>().text =
                _commander.isEquipped ? "EQUIPPED" : "EQUIP";
        }

        // Load commander image (in real implementation, this would download from URL)
        if (commanderImage != null)
        {
            // Placeholder - you'd implement image loading here
            commanderImage.color = GetRarityColor(_commander.rarity);
        }
    }

    string GetCommanderStats()
    {
        var traits = _commander.traits;
        return $"Leadership: {traits.leadership}/100\n" +
               $"Strategy: {traits.strategy}/100\n" +
               $"Loyalty: {traits.loyalty}/100\n" +
               $"Clan: {traits.clan}\n" +
               $"Era: {traits.era}\n" +
               $"Ability: {traits.specialAbility}";
    }

    Color GetRarityColor(Web3Manager.NFTCommander.CommanderRarity rarity)
    {
        switch (rarity)
        {
            case Web3Manager.NFTCommander.CommanderRarity.Common: return commonColor;
            case Web3Manager.NFTCommander.CommanderRarity.Uncommon: return uncommonColor;
            case Web3Manager.NFTCommander.CommanderRarity.Rare: return rareColor;
            case Web3Manager.NFTCommander.CommanderRarity.Epic: return epicColor;
            case Web3Manager.NFTCommander.CommanderRarity.Legendary: return legendaryColor;
            case Web3Manager.NFTCommander.CommanderRarity.Mythic: return mythicColor;
            default: return Color.white;
        }
    }

    void OnEquipClicked()
    {
        if (_web3Manager != null && !_commander.isEquipped)
        {
            _web3Manager.EquipCommander(_commander);
            UpdateUI();
        }
    }
}