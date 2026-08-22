using LogicLink.Generator;
using LogicLink.Generator.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolkist;
using ZeepSDK.LevelEditor;
using ZeepSDK.Messaging;

namespace LogicLink.LogicV2
{
    public static class SelectionOpperations
    {
        public static bool CanOpperateOnSelection()
        {
            return LevelEditorApi.IsInLevelEditor && Plugin.Central;
        }


        public static void SetLogicVisibility(bool visible)
        {
            if (!CanOpperateOnSelection()) return;

            UndoRedoInfo undoRedoInfo = CreateUndoRedoInfo();
            List<BlockEdit_v18_C_Logic_Base> logicBlocks = GetLogicBlocks(undoRedoInfo.Selection);
            if (logicBlocks.Count == 0)
            {
                MessengerApi.Log("No logic blocks selected!");
                return;
            }

            foreach (BlockEdit_v18_C_Logic_Base block in logicBlocks)
            {
                block.customProperties.SetBoolDictionaryValue(block.KEY_toggleHideInGame, !visible);
                if (block is BlockEdit_v18_C_Logic_Trigger trigger)
                {
                    trigger.customProperties.SetBoolDictionaryValue("xt1", !visible);
                    trigger.hideTrigger = !visible;
                    Plugin.Logger.LogInfo("TEST");
                }
            }

            ResolveUndoRedoInfo(undoRedoInfo);
            MessengerApi.Log($"Logic visibility set to {(visible ? "visible" : "hidden")}");
        }

        public static void CreateTogglers()
        {
            if (!CanOpperateOnSelection()) return;

            List<BlockProperties> selection = Plugin.Central.selection.list;
            if (selection.Count == 0)
            {
                MessengerApi.Log("Nothing selected!");
                return;
            }
            string[] ids = [.. selection.Select((block) => block.UID)];

            LevelEditorApi.ClearSelection();

            List<Block> blocks = TogglerGenerator.CreateTogglers(ids);
            ZeeplevelData blueprint = blocks.ToBlueprint("TogglerGenerator");
            List<BlockProperties> test = blueprint.PasteIntoEditor(Plugin.Central, false);
        }



        private static List<BlockEdit_v18_C_Logic_Base> GetLogicBlocks(List<BlockProperties> selection)
        {
            List<BlockEdit_v18_C_Logic_Base> logicBlocks = [];
            foreach (BlockProperties block in selection)
            {
                BlockEdit_v18_C_Logic_Base logicEdit = block.GetComponentInChildren<BlockEdit_v18_C_Logic_Base>();
                if (logicEdit)
                {
                    logicBlocks.Add(logicEdit);
                }
            }
            return logicBlocks;
        }

        private static UndoRedoInfo CreateUndoRedoInfo()
        {
            LEV_LevelEditorCentral central = Plugin.Central;
            LEV_UndoRedo undoRedo = central.undoRedo;
            List<BlockProperties> selection = central.selection.list;
            List<string> before = undoRedo.ConvertBlockListToJSONList(selection);
            List<string> beforeSelection = undoRedo.ConvertSelectionToStringList(selection);

            List<BlockEdit_v18_C_Logic_Base> logicBlocks = [];
            foreach (BlockProperties block in selection)
            {
                BlockEdit_v18_C_Logic_Base logicEdit = block.GetComponentInChildren<BlockEdit_v18_C_Logic_Base>();
                if (logicEdit)
                {
                    logicBlocks.Add(logicEdit);
                }
            }

            return new(selection, before, beforeSelection);
        }

        private static void ResolveUndoRedoInfo(UndoRedoInfo undoRedoInfo)
        {
            LEV_LevelEditorCentral central = Plugin.Central;
            LEV_UndoRedo undoRedo = central.undoRedo;

            List<string> after = undoRedo.ConvertBlockListToJSONList(undoRedoInfo.Selection);
            List<string> afterSelection = undoRedo.ConvertSelectionToStringList(undoRedoInfo.Selection);

            Change_Collection collection = undoRedo.ConvertBeforeAndAfterListToCollection(undoRedoInfo.Before, after, undoRedoInfo.Selection, undoRedoInfo.BeforeSelection, afterSelection);
            central.validation.BreakLock(collection, "LogicLink - SetLogicVisibility");

            central.selection.DeselectAllBlocks(false, "LogicLink - SetLogicVisibility");
            undoRedo.Reselect(collection, false);
        }


        private class UndoRedoInfo()
        {
            public List<BlockProperties> Selection;
            public List<string> Before;
            public List<string> BeforeSelection;

            public UndoRedoInfo(List<BlockProperties> selection, List<string> before, List<string> beforeSelection) : this()
            {
                Selection = selection;
                Before = before;
                BeforeSelection = beforeSelection;
            }
        }
    }
}
