using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Web3Manager;

public class Web3Manager : MonoBehaviour
{
    public static Web3Manager Instance { get; private set; }

    [Header("Web3 UI")]
    public GameObject web3Panel;
    public TextMeshProUGUI kachiBalanceText;
    public TextMeshProUGUI meiBalanceText;
    public Transform nftContainer;
    public GameObject nftItemPrefab;

    [Header("Web3 Settings")]
    public string gameContractAddress = "0x1234...";
    public string kachiTokenAddress = "0x5678...";
    public string meiTokenAddress = "0x9ABC...";

    [Header("Placeholder Data")]
    public double kachiBalance = 1000;
    public double meiBalance = 100;
    public List<NFTCommander> ownedCommanders = new List<NFTCommander>();

    [Header("Simulated NFTs")]
    public NFTCommander[] availableCommanders;

    [System.Serializable]
    public class NFTCommander
    {
        public string tokenId;
        public string name;
        public CommanderRarity rarity;
        public string imageUrl;
        public CommanderTraits traits;
        public bool isEquipped = false;

        public enum CommanderRarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary,
            Mythic
        }
    }

    [System.Serializable]
    public class CommanderTraits
    {
        public int leadership;
        public int strategy;
        public int loyalty;
        public string clan;
        public string era;
        public string specialAbility;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeWeb3Data();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Initialize(GameData data)
    {
        Debug.Log("✅ Web3Manager initialized");
    }

    void InitializeWeb3Data()
    {
        // Simulate owning some NFTs
        ownedCommanders.Clear();

        // Add a few starter commanders
        if (availableCommanders.Length >= 2)
        {
            ownedCommanders.Add(availableCommanders[0]); // Common
            ownedCommanders.Add(availableCommanders[3]); // Rare
        }

        Debug.Log("🌐 Web3 Manager initialized (Placeholder Mode)");
    }

    public void ShowWeb3Panel()
    {
        if (web3Panel != null)
        {
            UpdateWeb3UI();
            web3Panel.SetActive(true);
        }
    }

    public void HideWeb3Panel()
    {
        if (web3Panel != null)
        {
            web3Panel.SetActive(false);
        }
    }

    void UpdateWeb3UI()
    {
        // Update token balances
        if (kachiBalanceText != null)
            kachiBalanceText.text = $"{kachiBalance} ¥KACHI";

        if (meiBalanceText != null)
            meiBalanceText.text = $"{meiBalance} ¥MEI";

        // Update NFT display
        UpdateNFTDisplay();
    }

    void UpdateNFTDisplay()
    {
        // Clear existing NFTs
        foreach (Transform child in nftContainer)
        {
            Destroy(child.gameObject);
        }

        // Create NFT items
        foreach (var commander in ownedCommanders)
        {
            CreateNFTItem(commander);
        }
    }

    void CreateNFTItem(NFTCommander commander)
    {
        if (nftItemPrefab == null) return;

        var nftObj = Instantiate(nftItemPrefab, nftContainer);
        var nftUI = nftObj.GetComponent<NFTCommanderUI>();

        if (nftUI != null)
        {
            nftUI.Initialize(commander, this);
        }
    }

    // Simulated Web3 methods
    public void SimulateKachiEarnings(double amount)
    {
        kachiBalance += amount;
        UpdateWeb3UI();
        Debug.Log($"💰 Earned {amount} ¥KACHI (Utility Token)");
    }

    public void SimulateMeiEarnings(double amount)
    {
        meiBalance += amount;
        UpdateWeb3UI();
        Debug.Log($"🏛️ Earned {amount} ¥MEI (Governance Token)");
    }

    public void SimulateNFTPurchase()
    {
        // Find an unowned commander
        foreach (var commander in availableCommanders)
        {
            if (!ownedCommanders.Contains(commander))
            {
                ownedCommanders.Add(commander);
                UpdateWeb3UI();
                Debug.Log($"🎴 Purchased NFT Commander: {commander.name}");
                return;
            }
        }

        Debug.Log("❌ No more NFTs available for purchase");
    }

    public void EquipCommander(NFTCommander commander)
    {
        // Unequip currently equipped commander
        foreach (var cmd in ownedCommanders)
        {
            cmd.isEquipped = false;
        }

        // Equip new commander
        commander.isEquipped = true;

        // Apply commander bonuses
        ApplyCommanderBonuses(commander);

        UpdateWeb3UI();
        Debug.Log($"⚔️ Equipped Commander: {commander.name}");
    }

    void ApplyCommanderBonuses(NFTCommander commander)
    {
        // Apply bonuses based on commander traits
        double incomeBonus = 1.0 + (commander.traits.leadership * 0.01);
        float trainingBonus = 1.0f + (commander.traits.strategy * 0.005f);

        // Store these bonuses to be used in game calculations
        PlayerPrefs.SetFloat("CommanderIncomeBonus", (float)incomeBonus);
        PlayerPrefs.SetFloat("CommanderTrainingBonus", trainingBonus);

        Debug.Log($"📈 Commander bonuses applied: {incomeBonus}x income, {trainingBonus}x training");
    }

    public void SimulateTokenTransfer(string tokenType, double amount, string toAddress)
    {
        if (tokenType == "KACHI" && kachiBalance >= amount)
        {
            kachiBalance -= amount;
            Debug.Log($"➡️ Transferred {amount} ¥KACHI to {toAddress}");
        }
        else if (tokenType == "MEI" && meiBalance >= amount)
        {
            meiBalance -= amount;
            Debug.Log($"➡️ Transferred {amount} ¥MEI to {toAddress}");
        }
        else
        {
            Debug.Log("❌ Insufficient token balance for transfer");
        }

        UpdateWeb3UI();
    }

    public void SimulateStaking(string tokenType, double amount)
    {
        if (tokenType == "KACHI" && kachiBalance >= amount)
        {
            kachiBalance -= amount;
            Debug.Log($"🔒 Staked {amount} ¥KACHI");
            // In real implementation, you'd track staked amount and time
        }
        else if (tokenType == "MEI" && meiBalance >= amount)
        {
            meiBalance -= amount;
            Debug.Log($"🔒 Staked {amount} ¥MEI");
        }
        else
        {
            Debug.Log("❌ Insufficient token balance for staking");
        }

        UpdateWeb3UI();
    }
    // Add any Web3-specific initialization here
    public void ConnectWallet()
    {
        Debug.Log("🔗 Wallet connection placeholder");
    }

    public void CheckNFTs()
    {
        Debug.Log("🖼️ NFT check placeholder");
    }

    [ContextMenu("Add Random NFT")]
    public void AddRandomNFT()
    {
        SimulateNFTPurchase();
    }

    [ContextMenu("Earn Test Tokens")]
    public void EarnTestTokens()
    {
        SimulateKachiEarnings(100);
        SimulateMeiEarnings(10);
    }

    [ContextMenu("Debug Web3 Info")]
    public void DebugWeb3Info()
    {
        Debug.Log($"=== WEB3 DEBUG ===");
        Debug.Log($"¥KACHI Balance: {kachiBalance}");
        Debug.Log($"¥MEI Balance: {meiBalance}");
        Debug.Log($"Owned NFTs: {ownedCommanders.Count}");
        Debug.Log($"Equipped Commander: {GetEquippedCommander()?.name ?? "None"}");
        Debug.Log($"=== END DEBUG ===");
    }

    NFTCommander GetEquippedCommander()
    {
        return ownedCommanders.Find(c => c.isEquipped);
    }
}