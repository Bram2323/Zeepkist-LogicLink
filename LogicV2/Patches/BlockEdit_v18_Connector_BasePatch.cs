using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeepSDK.LevelEditor;
using ZeepSDK.Messaging;

namespace LogicLink.LogicV2.Patches
{
    [HarmonyPatch(typeof(BlockEdit_v18_Connector_Base), "UpdateThisBlockConnectionBase")]
    public static class BlockEdit_v18_Connector_Base_UpdateThisBlockConnectionBase
    {
        private static bool Prefix(List<ConnectionVisualizer> ___allConnectionVisualizers)
        {
            if (Plugin.Central == null || (Plugin.Central.tool.currentTool != 5 && Plugin.Central.connectorTool.hideInEditModeToggle.isOn)) return true;

            ConnectionVisibility.ConnectionVisibilityTypes connectionVisibility = ConnectionVisibility.CurrentConnectionVisibility;

            if (connectionVisibility == ConnectionVisibility.ConnectionVisibilityTypes.Default) return true;

            bool shouldHide = false;

            if (connectionVisibility == ConnectionVisibility.ConnectionVisibilityTypes.Never) shouldHide = true;

            if (shouldHide) HideConnection(___allConnectionVisualizers);
            return !shouldHide;
        }

        private static void HideConnection(List<ConnectionVisualizer> ___allConnectionVisualizers)
        {
            for (int i = 0; i < ___allConnectionVisualizers.Count; i++)
            {
                if (___allConnectionVisualizers[i] != null)
                {
                    UnityEngine.Object.Destroy(___allConnectionVisualizers[i].gameObject);
                }
            }
            ___allConnectionVisualizers.Clear();
        }
    }
}
