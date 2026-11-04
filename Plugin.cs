using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LogicLink.Generator.Generators.VideoToSigns;
using LogicLink.LogicV1;
using LogicLink.LogicV2;
using LogicLink.LogicV2.Drawers;
using LogicLink.LogicV2.Patches;
using LogicLink.Settings;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZeepSDK.LevelEditor;
using ZeepSDK.Messaging;
using ZeepSDK.Settings;
using ZeepSDK.UI;

namespace LogicLink;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("ZeepSDK")]
[BepInDependency("com.metalted.zeepkist.toolkist")]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance;
    public static new ManualLogSource Logger;
    public static bool PluginLoaded = false;

    public static LEV_LevelEditorCentral Central;

    private OldLogicPlugin OldLogicPlugin;

    private Harmony harmony;

    private GeneratorsDrawer GeneratorsDrawer = new();
    private RomGeneratorDrawer RomGeneratorDrawer = new();
    private VideoGeneratorDrawer VideoGeneratorDrawer = new();
    private MusicGeneratorDrawer MusicGeneratorDrawer = new();

    private Setting<KeyCode> OpenGenerators = new("Generators", "Open Generator Screen", KeyCode.None, "Open the generator select screen");

    private Setting<KeyCode> OpenRomGenerator = new("Generators", "Open Rom Generator", KeyCode.None, "Open the Rom Generator");
    private Setting<KeyCode> OpenVideoGenerator = new("Generators", "Open Video Generator", KeyCode.None, "Opens the Video Generator");
    private Setting<KeyCode> OpenMusicGenerator = new("Generators", "Open Music Generator", KeyCode.None, "Opens the Music Generator");

    private Setting<KeyCode> CreateTogglers = new("Shortcuts", "Create Togglers", KeyCode.None, "Create togglers for the current selection");
    private Setting<KeyCode> HideLogic = new("Shortcuts", "Hide Logic Blocks", KeyCode.None, "Set all logic blocks to hide in game");
    private Setting<KeyCode> ShowLogic = new("Shortcuts", "Show Logic Blocks", KeyCode.None, "Set all logic blocks to show in game");

    private Setting<KeyCode> ToggleTogglerVisualizer = new("Visuals", "Toggle Toggler Visualizers", KeyCode.None, "Toggles wether or not the visualizer lines for togglers spawn");
    private Setting<KeyCode> ToggleConnectionVisibility = new("Visuals", "Toggle Connection Visibility", KeyCode.None, "Toggles the connection visibility");

    private (Setting<KeyCode>, Drawer)[] Drawers = [];


    public void Awake()
    {
        Instance = this;
        Logger = base.Logger;

        harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        Drawers = [
            (OpenGenerators, GeneratorsDrawer),
            (OpenRomGenerator, RomGeneratorDrawer),
            (OpenVideoGenerator, VideoGeneratorDrawer),
            (OpenMusicGenerator, MusicGeneratorDrawer),
            ];

        GeneratorsDrawer.Generators = [RomGeneratorDrawer, VideoGeneratorDrawer, MusicGeneratorDrawer];

        foreach ((Setting<KeyCode> _, Drawer drawer) in Drawers)
        {
            UIApi.AddZeepGUIDrawer(drawer);
        }

        OldLogicPlugin = new(Config);

        LevelEditorApi.EnteredLevelEditor += EnteredLevelEditor;
        LevelEditorApi.ExitedLevelEditor += ExitedLevelEditor;
        LevelEditorApi.SelectionChanged += SelectionChanged;

        PluginLoaded = true;

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    public void Start()
    {
        SettingsManager.HandleBacklog();
    }


    public void Update()
    {
        MainThreadScheduler.Update();


        if (LevelEditorApi.IsInLevelEditor && !LevelEditorApi.IsKeyboardInputBlocked)
        {
            foreach ((Setting<KeyCode> key, Drawer drawer) in Drawers)
            {
                if (key.KeyDown()) drawer.Toggle();
            }

            if (SelectionOpperations.CanOpperateOnSelection())
            {
                if (CreateTogglers.KeyDown()) SelectionOpperations.CreateTogglers();
                if (HideLogic.KeyDown()) SelectionOpperations.SetLogicVisibility(false);
                if (ShowLogic.KeyDown()) SelectionOpperations.SetLogicVisibility(true);
            }

            if (ToggleTogglerVisualizer.KeyDown()) LogicScript_Door_VisualizeToggler.ShouldSkip = !LogicScript_Door_VisualizeToggler.ShouldSkip;

            if (ToggleConnectionVisibility.KeyDown()) ConnectionVisibility.SwitchType();
        }

        OldLogicPlugin.Update();
    }

    private void EnteredLevelEditor()
    {
        Logger.LogInfo("Entering level editor!");

        Central = GameObject.FindObjectsOfType<LEV_LevelEditorCentral>()[0];
    }

    private void ExitedLevelEditor()
    {
        Logger.LogInfo("Exiting level editor!");
        Central = null;

        ConnectionVisibility.ExitedLevelEditor();

        foreach ((Setting<KeyCode> _, Drawer drawer) in Drawers)
        {
            drawer.Close();
        }
    }

    private void SelectionChanged(List<BlockProperties> selection)
    {
        ConnectionVisibility.SelectionChanged(selection);
    }


    public void OnDestroy()
    {
        foreach ((Setting<KeyCode> _, Drawer drawer) in Drawers)
        {
            UIApi.RemoveZeepGUIDrawer(drawer);
        }


        OldLogicPlugin?.OnDestroy();

        harmony?.UnpatchSelf();
        harmony = null;
        LevelEditorApi.EnteredLevelEditor -= EnteredLevelEditor;
        LevelEditorApi.ExitedLevelEditor -= ExitedLevelEditor;
        LevelEditorApi.SelectionChanged -= SelectionChanged;
    }
}
