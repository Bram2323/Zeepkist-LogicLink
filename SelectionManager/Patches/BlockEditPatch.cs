using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogicLink;

[HarmonyPatch(typeof(BlockEdit), "PropertyBreakLock")]
public class BlockEdit_PropertyBreakLock
{
    private static bool Prefix()
    {
        if (SelectionManager.Instance != null && SelectionManager.Instance.DontBreakLock) return false;
        return true;
    }
}

