using LogicLink;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using ZeepSDK.Messaging;

namespace LogicLink.Selection;

public class SelectionManager
{
    public static readonly Color DeselectedColor = Color.gray;
    public const float LooseRotationDeadZone = 0.1f;

    public static SelectionManager Instance;
    public static MoveMode MoveMode = MoveMode.Combined;


    public LEV_LevelEditorCentral Central;
    public Dictionary<string, SelectedParts> SelectedLogicBlocks = [];

    public bool DontBreakLock { get; private set; } = false;



    public SelectionManager(LEV_LevelEditorCentral central)
    {
        Central = central;
    }


    public static void CycleMode()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            MoveMode = MoveMode switch
            {
                MoveMode.Combined => MoveMode.LooseRotation,
                MoveMode.Strict => MoveMode.Combined,
                MoveMode.LoosePosition => MoveMode.Strict,
                MoveMode.LooseRotation => MoveMode.LoosePosition,
                _ => MoveMode.Combined,
            };
        }
        else
        {
            MoveMode = MoveMode switch
            {
                MoveMode.Combined => MoveMode.Strict,
                MoveMode.Strict => MoveMode.LoosePosition,
                MoveMode.LoosePosition => MoveMode.LooseRotation,
                MoveMode.LooseRotation => MoveMode.Combined,
                _ => MoveMode.Combined,
            };
        }

        if (Instance != null && Instance.Central.selection.list.Count != 0)
        {
            Instance.PaintAllBlocks();
            Instance.UpdateGizmo();
            Instance.CalculateMiddlePivot(false);
        }

        MessengerApi.Log($"Move Mode set to {MoveMode.ToReadableString()}");
    }

    public void SelectHeads()
    {
        if (Central.selection.list.Count == 0) return;

        List<string> before = Central.undoRedo.ConvertSelectionToStringList(Central.selection.list);

        BlockProperties[] list = [.. Central.selection.list];
        SelectedLogicBlocks.Clear();

        foreach (BlockProperties block in list)
        {
            BlockEdit_LogicGate logicEdit = block.GetComponent<BlockEdit_LogicGate>();
            if (logicEdit == null) continue;
            v16BallLogicBrain logicBrain = logicEdit.ballLogicBrain;

            SelectedParts selectedParts = new(logicBrain.useTwoInputs) { Head = true };
            SelectedLogicBlocks[block.UID] = selectedParts;
        }

        List<string> after = Central.undoRedo.ConvertSelectionToStringList(Central.selection.list);
        Central.selection.RegisterManualSelectionBreakLock(before, after);

        PaintAllBlocks();

        UpdateGizmo();
        Central.selection.CalculateMiddlePivot(false);
    }

    public void SelectTriggers()
    {
        if (Central.selection.list.Count == 0) return;

        List<string> before = Central.undoRedo.ConvertSelectionToStringList(Central.selection.list);

        BlockProperties[] list = [.. Central.selection.list];
        SelectedLogicBlocks.Clear();

        foreach (BlockProperties block in list)
        {
            BlockEdit_LogicGate logicEdit = block.GetComponent<BlockEdit_LogicGate>();
            if (logicEdit == null) continue;
            v16BallLogicBrain logicBrain = logicEdit.ballLogicBrain;

            SelectedParts selectedParts = new(logicBrain.useTwoInputs) { Trigger1 = true, Trigger2 = true };
            SelectedLogicBlocks[block.UID] = selectedParts;
        }

        List<string> after = Central.undoRedo.ConvertSelectionToStringList(Central.selection.list);
        Central.selection.RegisterManualSelectionBreakLock(before, after);

        PaintAllBlocks();

        UpdateGizmo();
        Central.selection.CalculateMiddlePivot(false);
    }

    public void SelectCombined()
    {
        if (Central.selection.list.Count == 0) return;

        List<string> before = Central.undoRedo.ConvertSelectionToStringList(Central.selection.list);

        BlockProperties[] list = [.. Central.selection.list];
        SelectedLogicBlocks.Clear();

        foreach (BlockProperties block in list)
        {
            BlockEdit_LogicGate logicEdit = block.GetComponent<BlockEdit_LogicGate>();
            if (logicEdit == null) continue;
            v16BallLogicBrain logicBrain = logicEdit.ballLogicBrain;

            SelectedParts selectedParts = new(logicBrain.useTwoInputs) { Head = true, Trigger1 = true, Trigger2 = true };
            SelectedLogicBlocks[block.UID] = selectedParts;
        }

        List<string> after = Central.undoRedo.ConvertSelectionToStringList(Central.selection.list);
        Central.selection.RegisterManualSelectionBreakLock(before, after);

        PaintAllBlocks();

        UpdateGizmo();
        Central.selection.CalculateMiddlePivot(false);
    }


    public void ClickedOnPart(Transform part)
    {
        PartType partType = GetPartType(part, out string uid, out v16BallLogicBrain logicBrain);

        if (!Central.input.MultiSelect.buttonHeld)
        {
            Central.selection.DeselectAllBlocks(false, "LogicLink - ClickedOnPart");
        }

        if (IsBlockSelected(uid) && !SelectedLogicBlocks.ContainsKey(uid))
        {
            SelectedParts parts = new(logicBrain.useTwoInputs)
            {
                Head = true,
                Trigger1 = true,
                Trigger2 = true,
            };
            SelectedLogicBlocks.Add(uid, parts);
        }

        SelectedParts selectedParts;
        if (!Central.input.MultiSelect.buttonHeld || !SelectedLogicBlocks.ContainsKey(uid))
        {
            if (!SelectedLogicBlocks.ContainsKey(uid)) SelectedLogicBlocks[uid] = new(logicBrain.useTwoInputs);
            selectedParts = SelectedLogicBlocks[uid];
            SetPartSelected(selectedParts, partType, true);
            UpdateBlockSelected(logicBrain.properties);
            Central.selection.CalculateMiddlePivot(false);
            return;
        }

        selectedParts = SelectedLogicBlocks[uid];
        if (MoveMode != MoveMode.Combined)
        {
            TogglePartSelected(selectedParts, partType);
        }
        else
        {
            SetAllPartsSelected(selectedParts, false);
        }

        UpdateBlockSelected(logicBrain.properties);
        Central.selection.CalculateMiddlePivot(false);
    }


    public PartType GetPartType(Transform part, out string uid, out v16BallLogicBrain logicBrain)
    {
        uid = null;
        logicBrain = null;

        Transform parent = part.parent;
        BlockEdit_LogicGate logicEdit = parent.GetComponent<BlockEdit_LogicGate>();
        if (logicEdit == null)
        {
            Plugin.Logger.LogWarning("BlockEdit_LogicGate not found!");
            return PartType.Unkown;
        }
        logicBrain = logicEdit.ballLogicBrain;

        uid = logicBrain.properties.UID;

        if (logicBrain.moveBallSpawnerInsteadOfTrigger)
        {
            if (part.gameObject == logicBrain.input1selector) return PartType.Head;
            else return PartType.Trigger1;
        }

        if (part.gameObject == logicBrain.input1selector) return PartType.Trigger1;
        else if (part.gameObject == logicBrain.input2selector) return PartType.Trigger2;
        else return PartType.Head;
    }

    public void SetPartSelected(SelectedParts selectedParts, PartType partType, bool selected)
    {
        switch (partType)
        {
            case PartType.Head:
                selectedParts.Head = selected;
                break;
            case PartType.Trigger1:
                selectedParts.Trigger1 = selected;
                break;
            case PartType.Trigger2:
                selectedParts.Trigger2 = selected;
                break;
            case PartType.Unkown:
            default:
                break;
        }
    }

    public void SetAllPartsSelected(SelectedParts selectedParts, bool selected)
    {
        selectedParts.Head = selected;
        selectedParts.Trigger1 = selected;
        selectedParts.Trigger2 = selected;
    }

    public void TogglePartSelected(SelectedParts selectedParts, PartType partType)
    {
        switch (partType)
        {
            case PartType.Head:
                selectedParts.Head = !selectedParts.Head;
                break;
            case PartType.Trigger1:
                selectedParts.Trigger1 = !selectedParts.Trigger1;
                break;
            case PartType.Trigger2:
                selectedParts.Trigger2 = !selectedParts.Trigger2;
                break;
            case PartType.Unkown:
            default:
                break;
        }
    }


    public void UpdateBlockSelected(BlockProperties block)
    {
        if (!SelectedLogicBlocks.ContainsKey(block.UID)) return;
        SelectedParts selectedParts = SelectedLogicBlocks[block.UID];

        if (selectedParts.NoneSelected)
        {
            DeselectBlock(block);
            return;
        }

        SelectBlock(block);
        PaintBlock(block);
    }


    public bool IsBlockSelected(string uid)
    {
        foreach (BlockProperties block in Central.selection.list)
        {
            if (block.UID == uid) return true;
        }
        return false;
    }

    public void SelectBlock(BlockProperties block)
    {
        if (IsBlockSelected(block.UID)) return;

        Central.selection.AddThisBlock(block);
        UpdateGizmo();

        Plugin.Logger.LogMessage("Selected block!");
    }

    public void DeselectBlock(BlockProperties block)
    {
        SelectedLogicBlocks.Remove(block.UID);

        if (!IsBlockSelected(block.UID)) return;

        List<BlockProperties> list = Central.selection.list;
        for (int i = 0; i < list.Count; i++)
        {
            if (block == list[i])
            {
                Central.selection.RemoveBlockAt(i, false, false);
                break;
            }
        }

        UpdateGizmo();

        Plugin.Logger.LogMessage("Deselected block!");
    }


    public void PaintAllBlocks()
    {
        List<BlockProperties> list = Central.selection.list;

        foreach (BlockProperties block in list)
        {
            if (!SelectedLogicBlocks.ContainsKey(block.UID)) continue;
            PaintBlock(block);
        }
    }

    public void PaintBlock(BlockProperties block)
    {
        if (!SelectedLogicBlocks.ContainsKey(block.UID)) return;
        SelectedParts selectedParts = SelectedLogicBlocks[block.UID];
        Material selectionPaint = Central.selection.selectionMaterial;

        BlockEdit_LogicGate logicEdit = block.GetComponent<BlockEdit_LogicGate>();
        if (logicEdit == null)
        {
            Plugin.Logger.LogWarning("Could not find BlockEdit_LogicGate!");
            return;
        }
        v16BallLogicBrain logicBrain = logicEdit.ballLogicBrain;

        if (MoveMode == MoveMode.Combined)
        {
            PaintEverything(block, true, selectionPaint);
            return;
        }

        bool headSelected = selectedParts.Head;
        bool trigger1Selected = selectedParts.Trigger1;
        bool trigger2Selected = selectedParts.Trigger2;

        if (MoveMode == MoveMode.LooseRotation && selectedParts.AnyTrigger)
        {
            trigger1Selected = true;
            trigger2Selected = true;
        }

        if (logicBrain.moveBallSpawnerInsteadOfTrigger)
        {
            (trigger1Selected, headSelected) = (headSelected, trigger1Selected);
        }

        PaintEverything(block, headSelected, selectionPaint);
        PaintTrigger(logicBrain.input1glow, trigger1Selected, selectionPaint);
        if (logicBrain.useTwoInputs) PaintTrigger(logicBrain.input2glow, trigger2Selected, selectionPaint);
    }

    public void PaintEverything(BlockProperties block, bool selected, Material selectionPaint)
    {
        PaintEverything([.. block.dynamicSelectionRenderers], selected, selectionPaint);

        Properties_RoadPainter roadPainter = block.GetComponent<Properties_RoadPainter>();
        if (roadPainter != null) PaintEverything([.. roadPainter.renderers], selected, selectionPaint);
    }

    public void PaintEverything(Renderer[] renderers, bool selected, Material selectionPaint)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] array = new Material[renderers[i].materials.Length];
            for (int j = 0; j < renderers[i].materials.Length; j++)
            {
                array[j] = GetMaterial(selected, selectionPaint);
            }
            renderers[i].sharedMaterials = array;
        }
    }

    public void PaintTrigger(GameObject trigger, bool selected, Material selectionPaint)
    {
        MeshRenderer meshRenderer = trigger.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Plugin.Logger.LogWarning("MeshRenderer was not found!");
            return;
        }

        Material[] array = new Material[meshRenderer.materials.Length];
        for (int j = 0; j < meshRenderer.materials.Length; j++)
        {
            array[j] = GetMaterial(selected, selectionPaint);
        }
        meshRenderer.sharedMaterials = array;
    }

    public Material GetMaterial(bool selected, Material selectionPaint)
    {
        if (selected)
        {
            return new(selectionPaint)
            {
                color = Color.HSVToRGB(UnityEngine.Random.Range(0f, 100f) / 100f, 1f, 0.25f),
                name = UnityEngine.Random.Range(0, 10000).ToString()
            };
        }
        else
        {
            return new(selectionPaint)
            {
                color = Color.HSVToRGB(0f, 0f, -0.5f),
                name = UnityEngine.Random.Range(0, 10000).ToString()
            };
        }
    }


    public void UpdateGizmo()
    {
        List<BlockProperties> list = Central.selection.list;
        if (list.Count == 0) return;
        Central.gizmos.SetNewBlockGridHeight(list[^1].transform.position.y);
    }

    public void CalculateMiddlePivot(bool forceDefault)
    {
        LEV_Selection selection = Central.selection;
        List<BlockProperties> list = selection.list;

        if (list.Count == 0) return;

        if (list.Count > 1)
        {
            if (!Central.rotflip.pivotLastSelected || forceDefault)
            {
                Vector3 avgPosition = Vector3.zero;
                for (int i = 0; i < list.Count; i++)
                {
                    avgPosition += GetMiddleOfBlock(list[i]);
                }
                avgPosition /= list.Count;
                float gridXZ = Central.gizmos.gridXZ;
                float gridY = Central.gizmos.gridY;
                if (gridXZ == 0f)
                {
                    gridXZ = 0.2f;
                }
                if (gridY == 0f)
                {
                    gridY = 0.2f;
                }
                float x = Mathf.Round(avgPosition.x / gridXZ) * gridXZ;
                float y = Mathf.Round(avgPosition.y / gridY) * gridY;
                float z = Mathf.Round(avgPosition.z / gridXZ) * gridXZ;
                avgPosition = new Vector3(x, y, z);
                Central.gizmos.SetMotherPosition(avgPosition);
                return;
            }
            Central.gizmos.SetMotherPosition(Central.rotflip.GetSelectedPivot());
            return;
        }
        else
        {
            BlockProperties block = list[0];
            if (SelectedLogicBlocks.ContainsKey(block.UID))
            {
                Central.gizmos.SetMotherPosition(GetMiddleOfBlock(block));
                return;
            }

            if (!Central.rotflip.pivotLastSelected)
            {
                Central.gizmos.SetMotherPosition(block.transform.position);
                return;
            }
            Vector3 avgPosition = Vector3.zero;
            int amount = 0;
            for (int j = 0; j < block.transform.childCount; j++)
            {
                MeshCollider component = block.transform.GetChild(j).GetComponent<MeshCollider>();
                if (component != null && !component.isTrigger && !component.convex && component.gameObject.activeSelf)
                {
                    avgPosition += component.bounds.center;
                    amount++;
                }
            }
            if (amount == 0)
            {
                Central.gizmos.SetMotherPosition(block.transform.position);
                return;
            }
            Vector3 motherPosition = new(avgPosition.x / amount, avgPosition.y / amount, avgPosition.z / amount);
            Central.gizmos.SetMotherPosition(motherPosition);
            return;
        }
    }

    public Vector3 GetMiddleOfBlock(BlockProperties block)
    {
        string uid = block.UID;
        if (!SelectedLogicBlocks.ContainsKey(uid)) return block.transform.position;

        SelectedParts selectedParts = SelectedLogicBlocks[uid];
        v16BallLogicBrain logicBrain = block.GetComponentInChildren<v16BallLogicBrain>();

        if (logicBrain == null)
        {
            Plugin.Logger.LogWarning("Could not find LogicBrain!");
            return Vector3.zero;
        }

        Vector3 avgPosition = Vector3.zero;
        int amount = 0;
        if (selectedParts.Head)
        {
            avgPosition += block.transform.position;
            amount++;
        }
        if (selectedParts.Trigger1 || (MoveMode == MoveMode.LooseRotation && selectedParts.AnyTrigger))
        {
            if (logicBrain.moveBallSpawnerInsteadOfTrigger)
            {
                avgPosition += logicBrain.ballSpawnPosition.position;
            }
            else
            {
                avgPosition += logicBrain.input1selector.transform.position;
            }
            amount++;
        }
        if (selectedParts.Trigger2 || (MoveMode == MoveMode.LooseRotation && selectedParts.AnyTrigger && selectedParts.UseTwoInputs))
        {
            avgPosition += logicBrain.input2selector.transform.position;
            amount++;
        }

        if (amount == 0) return Vector3.zero;
        avgPosition /= amount;
        return avgPosition;
    }


    public void TranslateBlock(BlockProperties block, Vector3 ogTranslation)
    {
        string uid = block.UID;
        Transform transform = block.transform;
        if (!SelectedLogicBlocks.ContainsKey(uid) || MoveMode == MoveMode.Combined)
        {
            transform.Translate(ogTranslation, Space.World);
            return;
        }

        BlockEdit_LogicGate logicEdit = block.GetComponent<BlockEdit_LogicGate>();
        if (logicEdit == null)
        {
            Plugin.Logger.LogWarning("Could not find BlockEdit component!");
            return;
        }

        SelectedParts selectedParts = SelectedLogicBlocks[uid];
        if (selectedParts.AllSelected ||
            (MoveMode == MoveMode.LooseRotation && selectedParts.Head && selectedParts.AnyTrigger))
        {
            transform.Translate(ogTranslation, Space.World);
            return;
        }

        Vector3 translation = transform.InverseTransformVector(ogTranslation);
        if (MoveMode == MoveMode.Strict) translation.x = 0;

        float distance1 = logicEdit.distance1 * 16;
        float height1 = logicEdit.height1 * 16;
        float distance2 = logicEdit.distance2 * 16;
        float height2 = logicEdit.height2 * 16;

        Vector3 localTranslation = translation.Multiply(transform.localScale);
        if (selectedParts.Head)
        {
            Vector3 looseTranslation = localTranslation;
            looseTranslation.x = 0;
            transform.Translate(looseTranslation, Space.Self);

            distance1 -= translation.z;
            height1 -= translation.y;
            distance2 -= translation.z;
            height2 -= translation.y;
        }

        if (MoveMode == MoveMode.LoosePosition || (MoveMode == MoveMode.LooseRotation && selectedParts.Head))
        {
            Vector3 looseTranslation = localTranslation;
            looseTranslation.y = 0;
            looseTranslation.z = 0;
            transform.Translate(looseTranslation, Space.Self);
        }

        if (selectedParts.Trigger1 || (MoveMode == MoveMode.LooseRotation && selectedParts.AnyTrigger))
        {
            distance1 += translation.z;
            height1 += translation.y;
        }
        if (selectedParts.Trigger2 || (MoveMode == MoveMode.LooseRotation && selectedParts.AnyTrigger))
        {
            distance2 += translation.z;
            height2 += translation.y;
        }

        float xDistance = localTranslation.x;
        xDistance /= transform.localScale.z;
        if (MoveMode == MoveMode.LooseRotation && !Mathf.Approximately(xDistance, 0))
        {
            float avgDistance = distance1;
            if (selectedParts.UseTwoInputs)
            {
                avgDistance += distance2;
                avgDistance /= 2;
            }

            Vector2 newPoint = new(avgDistance, xDistance);

            if (newPoint.magnitude > LooseRotationDeadZone)
            {
                float degrees = Mathf.Atan2(newPoint.y, newPoint.x) * Mathf.Rad2Deg;
                if (selectedParts.Head) degrees = -degrees;
                float deltaDistance = newPoint.magnitude - avgDistance;

                distance1 += deltaDistance;
                distance2 += deltaDistance;

                transform.Rotate(Vector2.up, degrees);
            }
        }

        distance1 /= 16;
        height1 /= 16;
        distance2 /= 16;
        height2 /= 16;

        if (Mathf.Approximately(distance1, logicEdit.distance1) && Mathf.Approximately(height1, logicEdit.height1) &&
            Mathf.Approximately(distance2, logicEdit.distance2) && Mathf.Approximately(height2, logicEdit.height2))
        {
            return;
        }

        LEV_InspectorBridge bridge = logicEdit.properties2.bridge;
        bridge.SetFloatValue(logicEdit.NUMBER_distance1, distance1);
        bridge.SetFloatValue(logicEdit.NUMBER_height1, height1);
        bridge.SetFloatValue(logicEdit.NUMBER_distance2, distance2);
        bridge.SetFloatValue(logicEdit.NUMBER_height2, height2);

        DontBreakLock = true;
        logicEdit.LogicValueChanged();
        DontBreakLock = false;
    }


    public void OnDeselectEverything()
    {
        Plugin.Logger.LogMessage("Deselected everything!");
        SelectedLogicBlocks.Clear();
    }
}
