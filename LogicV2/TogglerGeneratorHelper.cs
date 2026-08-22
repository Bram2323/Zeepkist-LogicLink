using LogicLink.Generator;
using LogicLink.Generator.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using Toolkist;
using ZeepSDK.LevelEditor;
using ZeepSDK.Messaging;

namespace LogicLink.LogicV2
{
    public static class TogglerGeneratorHelper
    {
        public static bool CanCreateTogglers()
        {
            return LevelEditorApi.IsInLevelEditor && Plugin.Central;
        }

        public static void CreateTogglers()
        {
            if (!CanCreateTogglers()) return;

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
    }
}
