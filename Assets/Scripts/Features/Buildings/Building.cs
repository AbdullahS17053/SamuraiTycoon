using System.Collections.Generic;
using UnityEngine;

// Runtime building representation
public class Building
{
    public BuildingConfig Config { get; private set; }
    public BuildingData Data { get; private set; }
    public List<BuildingModule> Modules { get; private set; } = new List<BuildingModule>();
    public Dictionary<string, BuildingModuleData> ModuleData { get; private set; } = new Dictionary<string, BuildingModuleData>();

    // Events for building changes
    public System.Action<Building, int, int> OnBuildingUpgraded;
    public System.Action<Building, BuildingModule> OnModuleActivated;

    // Cache for performance
    private Dictionary<System.Type, BuildingModule> _moduleCache = new Dictionary<System.Type, BuildingModule>();

    public Building(BuildingConfig config, BuildingData data)
    {
        Config = config;
        Data = data;
        InitializeModules();
    }

    private void InitializeModules()
    {
        if (Config.modules == null)
        {
            Debug.LogWarning($"⚠️ No modules found for building: {Config.DisplayName}");
            return;
        }

        Modules.Clear();
        _moduleCache.Clear();

        foreach (var module in Config.modules)
        {
            if (module != null)
            {
                // Get or create module data
                string moduleKey = module.GetType().Name + "_" + module.moduleName;
                if (!ModuleData.ContainsKey(moduleKey))
                {
                    ModuleData[moduleKey] = new BuildingModuleData
                    {
                        moduleId = moduleKey,
                        level = 1,
                        isUnlocked = true
                    };
                }

                // Initialize module with data
                module.SetRuntimeData(ModuleData[moduleKey]);
                module.Initialize(ModuleData[moduleKey]);
                Modules.Add(module);

                // Cache module by type for faster access
                var moduleType = module.GetType();
                if (!_moduleCache.ContainsKey(moduleType))
                {
                    _moduleCache[moduleType] = module;
                }

                // Subscribe to module events
                module.OnModuleStarted += OnModuleStarted;
                module.OnModuleCompleted += OnModuleCompleted;
                module.OnModuleProgress += OnModuleProgress;

                Debug.Log($"🔧 Module {module.moduleName} initialized for {Config.DisplayName} (Level: {ModuleData[moduleKey].level})");
            }
        }
    }

    public void Tick(double deltaTime)
    {
        foreach (var module in Modules)
        {
            if (module.isActive && !module.IsMaxLevel())
            {
                module.OnBuildingTick(this, deltaTime);
            }
        }
    }

    public void OnUpgrade(int oldLevel, int newLevel)
    {
        Debug.Log($"⬆️ Building {Config.DisplayName} upgraded: {oldLevel} → {newLevel}");

        foreach (var module in Modules)
        {
            module.OnUpgrade(this, oldLevel, newLevel);
        }

        // Trigger event for UI updates
        OnBuildingUpgraded?.Invoke(this, oldLevel, newLevel);

        // Auto-save on upgrade
        TriggerAutoSave();
    }

    public void ActivateModule(BuildingModule module)
    {
        if (module != null && Modules.Contains(module))
        {
            module.OnButtonClick(this);
            OnModuleActivated?.Invoke(this, module);
            TriggerAutoSave();
        }
    }

    // Module event handlers
    private void OnModuleStarted(string buildingId)
    {
        Debug.Log($"🚀 Module started for {Config.DisplayName}");
    }

    private void OnModuleCompleted(string buildingId)
    {
        Debug.Log($"✅ Module completed for {Config.DisplayName}");
    }

    private void OnModuleProgress(string buildingId)
    {
        Debug.Log($"📈 Module progress for {Config.DisplayName}");
    }

    // Get specific module by type - optimized with cache
    public T GetModule<T>() where T : BuildingModule
    {
        var type = typeof(T);
        if (_moduleCache.ContainsKey(type))
        {
            return _moduleCache[type] as T;
        }

        // Fallback search
        foreach (var module in Modules)
        {
            if (module is T typedModule)
            {
                _moduleCache[type] = typedModule;
                return typedModule;
            }
        }
        return null;
    }

    // Get module by name
    public BuildingModule GetModule(string moduleName)
    {
        foreach (var module in Modules)
        {
            if (module.moduleName == moduleName)
                return module;
        }
        return null;
    }

    // Get building income with all modifiers
    public double GetTotalIncome()
    {
        if (!Data.IsUnlocked) return 0;

        double baseIncome = Config.GetIncome(Data.Level);
        double totalIncome = baseIncome;

        // Apply module modifiers
        var incomeModule = GetModule<IncomeModule>();
        if (incomeModule != null)
        {
            // Income modules typically add bonus income
            totalIncome += incomeModule.CalculateIncome(this);
        }

        // Apply speed module modifiers
        var speedModule = GetModule<SpeedModule>();
        if (speedModule != null)
        {
            totalIncome *= speedModule.GetCurrentSpeedMultiplier();
        }

        // Apply capacity module modifiers
        var capacityModule = GetModule<CapacityModule>();
        if (capacityModule != null && capacityModule.GetMaxCapacity(this) > 0)
        {
            double capacityEfficiency = (double)capacityModule.currentWorkers / capacityModule.GetMaxCapacity(this);
            totalIncome *= capacityEfficiency;
        }

        return totalIncome;
    }

    // Cleanup method to prevent memory leaks
    public void Cleanup()
    {
        foreach (var module in Modules)
        {
            if (module != null)
            {
                module.OnModuleStarted -= OnModuleStarted;
                module.OnModuleCompleted -= OnModuleCompleted;
                module.OnModuleProgress -= OnModuleProgress;
            }
        }

        Modules.Clear();
        _moduleCache.Clear();
        ModuleData.Clear();
    }

    private void TriggerAutoSave()
    {
        if (GameManager.Instance != null && GameManager.Instance.Save != null)
        {
            GameManager.Instance.Save.DelayedSave(2f);
        }
    }

    // Debug info
    public string GetDebugInfo()
    {
        return $"{Config.DisplayName} (Lvl {Data.Level}) - Income: {GetTotalIncome():F1}/s, Modules: {Modules.Count}";
    }

    [System.Obsolete("Use GetTotalIncome() instead")]
    public double GetBaseIncome()
    {
        return Config.GetIncome(Data.Level);
    }
}