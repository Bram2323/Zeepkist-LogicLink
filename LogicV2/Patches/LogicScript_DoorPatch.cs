using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using ZeepSDK.Messaging;

namespace LogicLink.LogicV2.Patches
{
    [HarmonyPatch(typeof(LogicScript_Door), "VisualizeToggler")]
    public class LogicScript_Door_VisualizeToggler
    {
        private static bool _shouldSkip = false;
        public static bool ShouldSkip
        {
            get => _shouldSkip;
            set
            {
                _shouldSkip = value;
                MessengerApi.Log($"Toggler visualizers are now {(_shouldSkip ? "hidden" : "visible")}!");
            }
        }

        private static bool Prefix(LogicScript_Door __instance)
        {
            if (ShouldSkip)
            {
                List<Transform> visualizers = __instance.togglerVisualizers;
                for (int i = 0; i < visualizers.Count; i++)
                {
                    GameObject.Destroy(visualizers[i].gameObject);
                }
                visualizers.Clear();

                return false;
            }
            return true;
        }
    }
}
