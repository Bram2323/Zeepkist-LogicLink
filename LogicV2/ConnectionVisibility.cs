using LogicLink.LogicV2.Patches;
using System.Collections.Generic;
using System.Linq;
using ZeepSDK.LevelEditor;
using ZeepSDK.Messaging;

namespace LogicLink.LogicV2
{
    public static class ConnectionVisibility
    {
        public static ConnectionVisibilityTypes CurrentConnectionVisibility { get; private set; } = ConnectionVisibilityTypes.Default;
        public static BlockProperties[] CachedSelection = [];
        public static bool Skip = false;

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
            RedrawConnections(blocks, false);
        }

        private static void RedrawConnections(List<BlockProperties> blocks, bool propogate)
        {
            List<BlockEdit_v18_Connector_Base> connectorBlocks = SelectionOpperations.GetConnectorBlocks(blocks);
            foreach (BlockEdit_v18_Connector_Base block in connectorBlocks)
            {
                BlockEdit_v18_Connector_BasePatch.UpdateThisBlockConnectionBase(block, "LogicLink - ConnectionVisualizer", propogate);
            }
        }

        public static void SelectionChanged(List<BlockProperties> selection)
        {
            if (Plugin.Central.tool.currentTool != 0 || Skip) return;

            List<BlockProperties> blocks = [];
            blocks.AddRange(GetRemovedBlocks(selection));
            blocks.AddRange(GetAddedBlocks(selection));

            CachedSelection = new BlockProperties[selection.Count];
            selection.CopyTo(CachedSelection);

            RedrawConnections(blocks, true);
        }

        public static void ExitedLevelEditor()
        {
            CachedSelection = [];
        }

        private static List<BlockProperties> GetRemovedBlocks(List<BlockProperties> selection)
        {
            List<BlockProperties> blocks = [];
            foreach (BlockProperties block in CachedSelection)
            {
                if (!selection.Contains(block)) blocks.Add(block);
            }
            return blocks;
        }

        private static List<BlockProperties> GetAddedBlocks(List<BlockProperties> selection)
        {
            List<BlockProperties> blocks = [];
            foreach (BlockProperties block in selection)
            {
                if (!CachedSelection.Contains(block)) blocks.Add(block);
            }
            return blocks;
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
