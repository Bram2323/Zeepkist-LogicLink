using HarmonyLib;
using LogicLink.Selection;
using System.Collections.Generic;

namespace LogicLink.Patches;

[HarmonyPatch(typeof(LEV_UndoRedo), "ConvertSelectionToStringList")]
public class LEV_UndoRedo_ConvertSelectionToStringList
{
    public static bool FromDeselectAll = false;

    public static bool Prefix(ref List<string> __result, List<BlockProperties> selection)
    {
        if (SelectionManager.Instance.SelectedLogicBlocks.Count == 0) return true;

        List<string> uids = [];

        Dictionary<string, SelectedParts> selectedLogicBlocks = SelectionManager.Instance.SelectedLogicBlocks;
        for (int i = 0; i < selection.Count; i++)
        {
            string uid = selection[i].UID;
            uids.Add(uid);

            if (!selectedLogicBlocks.TryGetValue(uid, out SelectedParts selectedParts)) continue;

            string selectedPartsString = selectedParts.ToUidString();

            uids.Add($"LogicLink_{selectedPartsString}_{uid}");
        }

        __result = uids;
        return false;
    }

    public static void Postfix()
    {
        if (!FromDeselectAll) return;

        SelectionManager.Instance.OnDeselectEverything();
        FromDeselectAll = false;
    }
}

[HarmonyPatch(typeof(LEV_UndoRedo), "Reselect")]
public class LEV_UndoRedo_Reselect
{
    public static void Prefix(Change_Collection changeCollection, bool before)
    {
        List<string> uidList;
        if (before) uidList = changeCollection.beforeSelectionUIDs;
        else uidList = changeCollection.afterSelectionUIDs;

        foreach (string uidFull in uidList)
        {
            if (!uidFull.StartsWith("LogicLink")) continue;

            string[] parts = uidFull.Split('_');

            SelectedParts selectedParts = SelectedParts.FromUidString(parts[1]);

            string uid = "";
            for (int i = 2; i < parts.Length; i++)
            {
                if (i != 2) uid += "_";
                uid += parts[i];
            }

            SelectionManager.Instance.SelectedLogicBlocks[uid] = selectedParts;
        }
    }

    public static void Postfix()
    {
        SelectionManager.Instance.PaintAllBlocks();
    }
}
