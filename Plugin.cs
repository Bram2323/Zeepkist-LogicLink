using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LogicLink.Plane;
using LogicLink.Selection;
using UnityEngine;
using ZeepSDK.LevelEditor;

namespace LogicLink;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("ZeepSDK")]
public class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;

    private Harmony harmony;

    ConfigEntry<KeyCode> ChangeMoveMode;
    ConfigEntry<KeyCode> SelectHeads;
    ConfigEntry<KeyCode> SelectTriggers;
    ConfigEntry<KeyCode> SelectCombined;
    ConfigEntry<KeyCode> ToggleTriggerPlane;


    private void Awake()
    {
        Logger = base.Logger;

        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        ChangeMoveMode = Config.Bind("Settings", "Change Move Mode", KeyCode.None, "Press to change the move mode");
        SelectHeads = Config.Bind("Settings", "Select Heads", KeyCode.None, "Select the heads from the current selection");
        SelectTriggers = Config.Bind("Settings", "Select Triggers", KeyCode.None, "Select the triggers from the current selection");
        SelectCombined = Config.Bind("Settings", "Select Combined", KeyCode.None, "Select both the heads and the triggers from the current selection");
        ToggleTriggerPlane = Config.Bind("Settings", "Toggle Trigger Plane", KeyCode.None, "Press to toggle a plane to visualize the available movement of a trigger");

        LevelEditorApi.EnteredLevelEditor += EnteredLevelEditor;
        LevelEditorApi.ExitedLevelEditor += ExitedLevelEditor;

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Update()
    {
        if (Input.GetKeyDown(ChangeMoveMode.Value)) SelectionManager.CycleMode();
        if (Input.GetKeyDown(SelectHeads.Value)) SelectionManager.Instance?.SelectHeads();
        if (Input.GetKeyDown(SelectTriggers.Value)) SelectionManager.Instance?.SelectTriggers();
        if (Input.GetKeyDown(SelectCombined.Value)) SelectionManager.Instance?.SelectCombined();

        if (Input.GetKeyDown(ToggleTriggerPlane.Value)) PlaneManager.Instance?.TogglePlane();
    }

    private void EnteredLevelEditor()
    {
        LEV_LevelEditorCentral central = FindObjectsOfType<LEV_LevelEditorCentral>()[0];
        SelectionManager.Instance = new(central);
    }

    private void ExitedLevelEditor()
    {
        SelectionManager.Instance = null;
    }


    private void OnDestroy()
    {
        harmony?.UnpatchSelf();
        harmony = null;
        LevelEditorApi.EnteredLevelEditor -= EnteredLevelEditor;
        LevelEditorApi.ExitedLevelEditor -= ExitedLevelEditor;
    }
}
