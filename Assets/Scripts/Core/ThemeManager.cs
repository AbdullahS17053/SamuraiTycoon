using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }

    [Header("Japanese Theme Settings")]
    public Color primaryColor = new Color(0.8f, 0.1f, 0.1f); // Deep red
    public Color secondaryColor = new Color(0.1f, 0.1f, 0.3f); // Deep blue
    public Color accentColor = new Color(1f, 0.8f, 0f); // Gold

    [Header ("Japanese Buildings")]
    public BuildingTheme[] buildingThemes;

    [Header("Japanese Troops")]
    public TroopTheme[] troopThemes;

    [Header("UI Theme")]
    public Font japaneseFont;
    public Material uiBackgroundMaterial;
    public AudioClip japaneseBackgroundMusic;

    private Dictionary<string, BuildingTheme> _buildingThemeDict = new Dictionary<string, BuildingTheme>();
    private Dictionary<TroopUnit.TroopType, TroopTheme> _troopThemeDict = new Dictionary<TroopUnit.TroopType, TroopTheme>();

    [System.Serializable]
    public class BuildingTheme
    {
        public string buildingId;
        public string japaneseName;
        public string description;
        public Sprite japaneseIcon;
        public Color buildingColor;
        public AudioClip buildingSound;
    }

    [System.Serializable]
    public class TroopTheme
    {
        public TroopUnit.TroopType troopType;
        public string japaneseName;
        public string description;
        public Color uniformColor;
        public AudioClip battleCry;
        public GameObject japaneseModel;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeThemes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeThemes()
    {
        // Initialize building themes
        foreach (var theme in buildingThemes)
        {
            _buildingThemeDict[theme.buildingId] = theme;
        }

        // Initialize troop themes
        foreach (var theme in troopThemes)
        {
            _troopThemeDict[theme.troopType] = theme;
        }

        Debug.Log("🎌 Japanese theme manager initialized");
    }

    public BuildingTheme GetBuildingTheme(string buildingId)
    {
        return _buildingThemeDict.ContainsKey(buildingId) ? _buildingThemeDict[buildingId] : null;
    }

    public TroopTheme GetTroopTheme(TroopUnit.TroopType troopType)
    {
        return _troopThemeDict.ContainsKey(troopType) ? _troopThemeDict[troopType] : null;
    }

    public string GetJapaneseBuildingName(string buildingId)
    {
        var theme = GetBuildingTheme(buildingId);
        return theme != null ? theme.japaneseName : buildingId;
    }

    public string GetJapaneseTroopName(TroopUnit.TroopType troopType)
    {
        var theme = GetTroopTheme(troopType);
        return theme != null ? theme.japaneseName : troopType.ToString();
    }

    public void ApplyJapaneseThemeToBuilding(GameObject buildingObj, string buildingId)
    {
        var theme = GetBuildingTheme(buildingId);
        if (theme == null) return;

        // Apply color scheme
        var renderers = buildingObj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material.color = theme.buildingColor;
        }

        // Add Japanese architectural elements
        AddJapaneseDetails(buildingObj);
    }

    public void ApplyJapaneseThemeToTroop(GameObject troopObj, TroopUnit.TroopType troopType)
    {
        var theme = GetTroopTheme(troopType);
        if (theme == null) return;

        // Apply uniform color
        var renderers = troopObj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material.color = theme.uniformColor;
        }

        // Replace model with Japanese version if available
        if (theme.japaneseModel != null)
        {
            // Implementation depends on your model setup
        }
    }

    void AddJapaneseDetails(GameObject building)
    {
        // Add Japanese architectural details like curved roofs, paper screens, etc.
        // This would depend on your 3D models
    }

    [ContextMenu("Apply Japanese Theme To All")]
    public void ApplyJapaneseThemeToAll()
    {
        Debug.Log("🎌 Applying Japanese theme to all buildings and troops...");

        // Apply to buildings
        var buildingObjects = FindObjectsOfType<BuildingObject3D>();
        foreach (var building in buildingObjects)
        {
            ApplyJapaneseThemeToBuilding(building.gameObject, building.BuildingID);
        }

        // Apply to troops
        var troops = FindObjectsOfType<TroopUnit>();
        foreach (var troop in troops)
        {
            ApplyJapaneseThemeToTroop(troop.gameObject, troop.troopType);
        }

        // Apply UI theme
        ApplyJapaneseUITheme();

        Debug.Log("🎌 Japanese theme applied successfully!");
    }

    void ApplyJapaneseUITheme()
    {
        // Apply Japanese style to UI elements
        var textElements = FindObjectsOfType<TMPro.TextMeshProUGUI>();
        foreach (var text in textElements)
        {
            if (japaneseFont != null)
            {
                //text.font = japaneseFont;
            }
            text.color = primaryColor;
        }

        // Play Japanese background music
        var audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = japaneseBackgroundMusic;
        audioSource.loop = true;
        audioSource.Play();
    }
}