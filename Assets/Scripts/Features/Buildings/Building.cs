using System.Collections.Generic;
using UnityEngine;

// Runtime building representation
public class Building
{
    public BuildingConfig Config { get; private set; }
    public BuildingData Data { get; private set; }
    public List<BuildingModule> Modules { get; private set; } = new List<BuildingModule>();
    public Dictionary<string, BuildingModuleData> ModuleData { get; private set; } = new Dictionary<string, BuildingModuleData>();

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
            Debug.LogWarning($"No modules found for building: {Config.DisplayName}");
            return;
        }

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
            if (module.isActive)
            {
                module.OnBuildingTick(this, deltaTime);
            }
        }
    }

    public void OnUpgrade(int oldLevel, int newLevel)
    {
        foreach (var module in Modules)
        {
            module.OnUpgrade(this, oldLevel, newLevel);
        }
    }

    // Module event handlers
    private void OnModuleStarted(string buildingId)
    {
        Debug.Log($"🎬 Module started for {Config.DisplayName}");
    }

    private void OnModuleCompleted(string buildingId)
    {
        Debug.Log($"✅ Module completed for {Config.DisplayName}");
    }

    private void OnModuleProgress(string buildingId)
    {
        Debug.Log($"📈 Module progress for {Config.DisplayName}");
    }

    // Get specific module by type
    public T GetModule<T>() where T : BuildingModule
    {
        foreach (var module in Modules)
        {
            if (module is T typedModule)
                return typedModule;
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
}