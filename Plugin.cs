using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LogicLink.Plane;
using LogicLink.Selection;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZeepSDK.LevelEditor;

namespace LogicLink;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("ZeepSDK")]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance;
    public static new ManualLogSource Logger;


    private Harmony harmony;

    public ConfigEntry<KeyCode> ChangeMoveMode;
    public ConfigEntry<KeyCode> SelectHeads;
    public ConfigEntry<KeyCode> SelectTriggers;
    public ConfigEntry<KeyCode> SelectCombined;
    public ConfigEntry<KeyCode> HideTriggers;
    public ConfigEntry<KeyCode> ShowTriggers;
    public ConfigEntry<KeyCode> ToggleTriggerPlane;
    public ConfigEntry<bool> AlwaysShowPlane;
    public ConfigEntry<string> PlaneColor;
    public Color DefaultPlaneColor = new(1, 1, 1, 0.5f);


    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;

        harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        ChangeMoveMode = Config.Bind("Move Mode", "Change Move Mode", KeyCode.None, "Press to change the move mode");
        SelectHeads = Config.Bind("Move Mode", "Select Heads", KeyCode.None, "Select the heads from the current selection");
        SelectTriggers = Config.Bind("Move Mode", "Select Triggers", KeyCode.None, "Select the triggers from the current selection");
        SelectCombined = Config.Bind("Move Mode", "Select Combined", KeyCode.None, "Select both the heads and the triggers from the current selection");

        HideTriggers = Config.Bind("Selection", "Set Hide Triggers", KeyCode.None, "All selected logic blocks get set to hide triggers");
        ShowTriggers = Config.Bind("Selection", "Set Show Triggers", KeyCode.None, "All selected logic blocks get set to show triggers");

        ToggleTriggerPlane = Config.Bind("Plane", "Toggle Plane", KeyCode.None, "Toggle a plane to visualize the available movement of a trigger");
        AlwaysShowPlane = Config.Bind("Plane", "Always Show Plane", false, "Controls if the plane is shown on non logic blocks");
        PlaneColor = Config.Bind("Plane", "Plane Color", ColorUtility.ToHtmlStringRGBA(DefaultPlaneColor), "The color of the plane");

        PlaneColor.SettingChanged += PlaneColorChanged;

        LevelEditorApi.EnteredLevelEditor += EnteredLevelEditor;
        LevelEditorApi.ExitedLevelEditor += ExitedLevelEditor;
        LevelEditorApi.SelectionChanged += SelectionChanged;

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Update()
    {
        if (Input.GetKeyDown(ChangeMoveMode.Value)) SelectionManager.CycleMode();
        if (Input.GetKeyDown(SelectHeads.Value)) SelectionManager.Instance?.SelectHeads();
        if (Input.GetKeyDown(SelectTriggers.Value)) SelectionManager.Instance?.SelectTriggers();
        if (Input.GetKeyDown(SelectCombined.Value)) SelectionManager.Instance?.SelectCombined();

        if (Input.GetKeyDown(HideTriggers.Value)) SelectionManager.Instance?.HideTriggers();
        if (Input.GetKeyDown(ShowTriggers.Value)) SelectionManager.Instance?.ShowTriggers();

        if (Input.GetKeyDown(ToggleTriggerPlane.Value)) PlaneManager.Instance?.TogglePlane();
    }

    private void EnteredLevelEditor()
    {
        Logger.LogInfo("Entering level editor!");
        LEV_LevelEditorCentral central = FindObjectsOfType<LEV_LevelEditorCentral>()[0];
        SelectionManager.Instance = new(central);
        PlaneManager.Instance = new(central.selection, ParsePlaneColor());
    }

    private void ExitedLevelEditor()
    {
        Logger.LogInfo("Exiting level editor!");
        SelectionManager.Instance = null;
        PlaneManager.Instance = null;
    }

    public void SelectionChanged(List<BlockProperties> selection)
    {
        PlaneManager.Instance?.SelectionChanged();
    }

    public void PlaneColorChanged(object sender, EventArgs e)
    {
        PlaneManager.Instance?.ColorChanged(ParsePlaneColor());
    }

    public Color ParsePlaneColor()
    {
        string colorString = PlaneColor.Value;
        if (!colorString.StartsWith("#")) colorString = "#" + colorString;
        if (!ColorUtility.TryParseHtmlString(colorString, out Color color))
        {
            Logger.LogWarning($"Could not parse {colorString} to a color, using default color instead!");
            return DefaultPlaneColor;
        }
        return color;
    }


    private void OnDestroy()
    {
        harmony?.UnpatchSelf();
        harmony = null;
        LevelEditorApi.EnteredLevelEditor -= EnteredLevelEditor;
        LevelEditorApi.ExitedLevelEditor -= ExitedLevelEditor;
    }
}
