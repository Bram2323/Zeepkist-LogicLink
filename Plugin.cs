using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LogicLink.Generator.Generators.VideoToSigns;
using LogicLink.LogicV1;
using LogicLink.LogicV2;
using LogicLink.Settings;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZeepSDK.LevelEditor;
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

    private RomGeneratorDrawer RomGeneratorDrawer = new();
    private VideoGeneratorDrawer VideoGeneratorDrawer = new();

    private Setting<KeyCode> OpenRomGenerator = new("Generators", "Open Rom Generator", KeyCode.None, "Open the Rom Generator");

    private Setting<KeyCode> CreateTogglers = new("Generators", "Create Togglers", KeyCode.None, "Create togglers for the current selection");
    private Setting<KeyCode> HideLogic = new("Generators", "Hide Logic Blocks", KeyCode.None, "Set all logic blocks to hide in game");
    private Setting<KeyCode> ShowLogic = new("Generators", "Show Logic Blocks", KeyCode.None, "Set all logic blocks to show in game");

    private Setting<KeyCode> VideoGenerator = new("Generators", "Open Video Generator", KeyCode.None, "Opens the Video Generator");


    public void Awake()
    {
        Instance = this;
        Logger = base.Logger;

        harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        UIApi.AddZeepGUIDrawer(RomGeneratorDrawer);
        UIApi.AddZeepGUIDrawer(VideoGeneratorDrawer);

        OldLogicPlugin = new(Config);

        LevelEditorApi.EnteredLevelEditor += EnteredLevelEditor;
        LevelEditorApi.ExitedLevelEditor += ExitedLevelEditor;

        PluginLoaded = true;
        SettingsManager.HandleBacklog();

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    public void Update()
    {
        MainThreadScheduler.Update();

        if (OpenRomGenerator.KeyDown() && !LevelEditorApi.IsKeyboardInputBlocked)
        {
            if (RomGeneratorDrawer.CanOpen()) RomGeneratorDrawer.Open();
            else if (RomGeneratorDrawer.IsOpen()) RomGeneratorDrawer.Close();
        }

        if (VideoGenerator.KeyDown() && !LevelEditorApi.IsKeyboardInputBlocked)
        {
            if (VideoGeneratorDrawer.CanOpen()) VideoGeneratorDrawer.Open();
            else if (VideoGeneratorDrawer.IsOpen()) VideoGeneratorDrawer.Close();
        }

        if (SelectionOpperations.CanOpperateOnSelection() && !LevelEditorApi.IsKeyboardInputBlocked)
        {
            if (CreateTogglers.KeyDown()) SelectionOpperations.CreateTogglers();
            if (HideLogic.KeyDown()) SelectionOpperations.SetLogicVisibility(false);
            if (ShowLogic.KeyDown()) SelectionOpperations.SetLogicVisibility(true);
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
        RomGeneratorDrawer?.Close();
        VideoGeneratorDrawer?.Close();
    }


    public void OnDestroy()
    {
        UIApi.RemoveZeepGUIDrawer(RomGeneratorDrawer);
        UIApi.RemoveZeepGUIDrawer(VideoGeneratorDrawer);


        OldLogicPlugin?.OnDestroy();

        harmony?.UnpatchSelf();
        harmony = null;
        LevelEditorApi.EnteredLevelEditor -= EnteredLevelEditor;
        LevelEditorApi.ExitedLevelEditor -= ExitedLevelEditor;
    }
}
