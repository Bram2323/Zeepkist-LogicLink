using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogicLink;

[HarmonyPatch(typeof(LEV_ClickScript), "Update")]
public class LEV_ClickScript_Update
{
    private static void Postfix(LEV_ClickScript __instance)
    {

    }
}

