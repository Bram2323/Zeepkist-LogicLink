using System.Collections.Generic;
using ZeepSDK.LevelEditor;
using ZeepSDK.Messaging;

namespace LogicLink.LogicV2
{
    public static class ConnectionVisibility
    {

        public static ConnectionVisibilityTypes CurrentConnectionVisibility { get; private set; } = ConnectionVisibilityTypes.Default;

        public static void SwitchType()
        {
            CurrentConnectionVisibility = CurrentConnectionVisibility switch
            {
                ConnectionVisibilityTypes.Default => ConnectionVisibilityTypes.OnlySelected,
                ConnectionVisibilityTypes.OnlySelected => ConnectionVisibilityTypes.OnlyOpaque,
                ConnectionVisibilityTypes.OnlyOpaque => ConnectionVisibilityTypes.Never,
                ConnectionVisibilityTypes.Never => ConnectionVisibilityTypes.Default,
                _ => ConnectionVisibilityTypes.Default
            };

            RedrawConnections();
            MessengerApi.Log($"Connection visibility set to {GetConnectionString(CurrentConnectionVisibility)}!");
        }

        private static void RedrawConnections()
        {
            if (!LevelEditorApi.IsInLevelEditor || !Plugin.Central) return;

            List<BlockProperties> blocks = Plugin.Central.saveload.GetAllBlockPropertiesCurrentlyInLevel();
            List<BlockEdit_v18_Connector_Base> connectorBlocks = SelectionOpperations.GetConnectorBlocks(blocks);

            foreach (BlockEdit_v18_Connector_Base block in connectorBlocks)
            {
                block.ForceRedrawConnectionVisualizers("LogicLink - ConnectionVisualizer");
            }
        }

        private static string GetConnectionString(ConnectionVisibilityTypes connectionVisibility)
        {
            return connectionVisibility switch
            {
                ConnectionVisibilityTypes.Default => "default",
                ConnectionVisibilityTypes.OnlySelected => "only selected",
                ConnectionVisibilityTypes.OnlyOpaque => "only opaque",
                ConnectionVisibilityTypes.Never => "never",
                _ => "None"
            };
        }


        public enum ConnectionVisibilityTypes
        {
            Default,
            OnlySelected,
            OnlyOpaque,
            Never,
        }
    }
}
