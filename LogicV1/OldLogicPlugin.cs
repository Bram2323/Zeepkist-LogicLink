using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LogicLink.LogicV1.Plane;
using LogicLink.LogicV1.Selection;
using LogicLink.Settings;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZeepSDK.LevelEditor;

namespace LogicLink.LogicV1
{
    public class OldLogicPlugin
    {
        public static OldLogicPlugin Instance { get; private set; }

        public Setting<KeyCode> ChangeMoveMode = new("Move Mode", "Change Move Mode", KeyCode.None, "Press to change the move mode", "LogicV1");
        public Setting<KeyCode> SelectHeads = new("Move Mode", "Select Heads", KeyCode.None, "Select the heads from the current selection", "LogicV1");
        public Setting<KeyCode> SelectTriggers = new("Move Mode", "Select Triggers", KeyCode.None, "Select the triggers from the current selection", "LogicV1");
        public Setting<KeyCode> SelectCombined = new("Move Mode", "Select Combined", KeyCode.None, "Select both the heads and the triggers from the current selection", "LogicV1");
        public Setting<KeyCode> HideTriggers = new("Selection", "Set Hide Triggers", KeyCode.None, "All selected logic blocks get set to hide triggers", "LogicV1");
        public Setting<KeyCode> ShowTriggers = new("Selection", "Set Show Triggers", KeyCode.None, "All selected logic blocks get set to show triggers", "LogicV1");
        public Setting<KeyCode> ToggleTriggerPlane = new("Plane", "Toggle Plane", KeyCode.None, "Toggle a plane to visualize the available movement of a trigger", "LogicV1");
        public Setting<bool> AlwaysShowPlane = new("Plane", "Always Show Plane", false, "Controls if the plane is shown on non logic blocks", "LogicV1");
        public Setting<string> PlaneColor;
        public Color DefaultPlaneColor = new(1, 1, 1, 0.5f);


        public OldLogicPlugin(ConfigFile config)
        {
            Instance = this;

            PlaneColor = new("Plane", "Plane Color", ColorUtility.ToHtmlStringRGBA(DefaultPlaneColor), "The color of the plane", "LogicV1");
            PlaneColor.SettingChanged += PlaneColorChanged;

            LevelEditorApi.EnteredLevelEditor += EnteredLevelEditor;
            LevelEditorApi.ExitedLevelEditor += ExitedLevelEditor;
            LevelEditorApi.SelectionChanged += SelectionChanged;
        }

        public void Update()
        {
            if (Input.GetKeyDown(ChangeMoveMode.Value)) OldSelectionManager.CycleMode();
            if (Input.GetKeyDown(SelectHeads.Value)) OldSelectionManager.Instance?.SelectHeads();
            if (Input.GetKeyDown(SelectTriggers.Value)) OldSelectionManager.Instance?.SelectTriggers();
            if (Input.GetKeyDown(SelectCombined.Value)) OldSelectionManager.Instance?.SelectCombined();

            if (Input.GetKeyDown(HideTriggers.Value)) OldSelectionManager.Instance?.HideTriggers();
            if (Input.GetKeyDown(ShowTriggers.Value)) OldSelectionManager.Instance?.ShowTriggers();

            if (Input.GetKeyDown(ToggleTriggerPlane.Value)) PlaneManager.Instance?.TogglePlane();
        }


        private void EnteredLevelEditor()
        {
            LEV_LevelEditorCentral central = GameObject.FindObjectsOfType<LEV_LevelEditorCentral>()[0];
            OldSelectionManager.Instance = new(central);
            PlaneManager.Instance = new(central.selection, ParsePlaneColor());
        }

        private void ExitedLevelEditor()
        {
            OldSelectionManager.Instance = null;
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
                Plugin.Logger.LogWarning($"Could not parse {colorString} to a color, using default color instead!");
                return DefaultPlaneColor;
            }
            return color;
        }


        public void OnDestroy()
        {
            LevelEditorApi.EnteredLevelEditor -= EnteredLevelEditor;
            LevelEditorApi.ExitedLevelEditor -= ExitedLevelEditor;
        }
    }
}
