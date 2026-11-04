using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ZeepSDK.LevelEditor;
using static Imui.Examples.ImConsoleWindow;

namespace LogicLink.LogicV2.Patches
{
    [HarmonyPatch(typeof(BlockEdit_v18_Connector_Base), "UpdateThisBlockConnectionBase")]
    public static class BlockEdit_v18_Connector_Base_UpdateThisBlockConnectionBase
    {
        private const string HideInGameKey = "lbhd";

        private static bool Prefix(BlockEdit_v18_Connector_Base __instance, bool propagate, List<ConnectionVisualizer> ___allConnectionVisualizers)
        {
            if (!LevelEditorApi.IsInLevelEditor || Plugin.Central == null ||
                (Plugin.Central.tool.currentTool != 5 && Plugin.Central.connectorTool.hideInEditModeToggle.isOn)) return true;

            bool ShouldRun = HandleCustomVisualizer(__instance, propagate, ___allConnectionVisualizers);
            return ShouldRun;
        }

        private static bool HandleCustomVisualizer(BlockEdit_v18_Connector_Base instance, bool propagate, List<ConnectionVisualizer> visualizers)
        {
            ConnectionVisibility.ConnectionVisibilityTypes connectionVisibility = ConnectionVisibility.CurrentConnectionVisibility;

            if (connectionVisibility == ConnectionVisibility.ConnectionVisibilityTypes.Default) return true;

            bool shouldHide = false;

            if (connectionVisibility == ConnectionVisibility.ConnectionVisibilityTypes.Never) shouldHide = true;
            else if (connectionVisibility == ConnectionVisibility.ConnectionVisibilityTypes.OnlySelected)
            {
                ShowConnections(instance, propagate, visualizers, (connection) =>
                {
                    BlockProperties block = StaticConnectorTracker.allBlocksByUID[connection.targetUID];
                    return block == null || IsInSelection(instance.GetBlockPropertiesScript()) || IsInSelection(block);
                });
                return false;
            }
            else if (connectionVisibility == ConnectionVisibility.ConnectionVisibilityTypes.OnlyOpaque)
            {
                bool isHideInGame = IsHideInGame(instance.customProperties);
                ShowConnections(instance, propagate, visualizers, (connection) =>
                {
                    BlockProperties block = StaticConnectorTracker.allBlocksByUID[connection.targetUID];
                    return block == null || (!isHideInGame && !IsHideInGame(block.customBlockEditProperties));
                });
                return false;
            }

            if (shouldHide) HideConnection(visualizers);
            return !shouldHide;
        }

        private static void ShowConnections(BlockEdit_v18_Connector_Base instance, bool propagate, List<ConnectionVisualizer> visualizers, Func<ConnectionStruct, bool> predicate)
        {
            HideConnection(visualizers);
            for (int j = 0; j < instance.localConnectorHubs.Count; j++)
            {
                ConnectorHub_Base connectorHub = instance.localConnectorHubs[j];
                for (int k = 0; k < instance.GetSpecificLocalConnHubList(j).Count; k++)
                {
                    if (StaticConnectorTracker.allBlocksByUID.ContainsKey(instance.GetSpecificLocalConnHubList(j)[k].targetUID))
                    {
                        ConnectionStruct connection = instance.GetSpecificLocalConnHubList(j)[k];
                        if (!connection.drawingLeader || predicate(connection)) instance.DrawAllConnectionVisualizers(connectorHub, connection, propagate);
                    }
                }
            }
        }

        private static void HideConnection(List<ConnectionVisualizer> visualizers)
        {
            for (int i = 0; i < visualizers.Count; i++)
            {
                if (visualizers[i] != null)
                {
                    UnityEngine.Object.Destroy(visualizers[i].gameObject);
                }
            }
            visualizers.Clear();
        }


        private static bool IsInSelection(BlockProperties block)
        {
            return ConnectionVisibility.CachedSelection.Contains(block);
        }


        private static bool IsHideInGame(PropertyDictionariesJSON properties)
        {
            return properties.ContainsIntKey(HideInGameKey) && properties.GetBoolDictionaryValue(HideInGameKey);
        }
    }

    public class BlockEdit_v18_Connector_BasePatch
    {
        public static void UpdateThisBlockConnectionBase(BlockEdit_v18_Connector_Base instance, string debugSource, bool propogate)
        {
            MethodInfo method = instance.GetType().GetMethod("UpdateThisBlockConnectionBase", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(instance, [debugSource, propogate]);
        }
    }
}
