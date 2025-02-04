using HarmonyLib;
using System;
using System.Collections.Generic;

namespace LogicLink.SelectionManager.Patches;

[HarmonyPatch(typeof(EditObjects), "Update")]
public class EditObjects_Update
{

}

