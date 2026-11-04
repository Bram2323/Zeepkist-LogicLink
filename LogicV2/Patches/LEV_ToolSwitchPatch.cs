using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.LogicV2.Patches
{
    [HarmonyPatch(typeof(LEV_ToolSwitch), "DisableAllTools")]
    public class LEV_ToolSwitch_DisableAllToolsPatch
    {
        private static void Prefix()
        {
            ConnectionVisibility.Skip = true;
        }

        private static void Postfix()
        {
            ConnectionVisibility.Skip = false;
        }
    }
}
