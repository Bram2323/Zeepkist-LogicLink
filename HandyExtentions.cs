using BepInEx.Configuration;
using LogicLink.Generator;
using LogicLink.LogicV1.Selection;
using LogicLink.Settings;
using System.Collections.Generic;
using System.Linq;
using Toolkist;
using Toolkist.EditorOperations;
using UnityEngine;

namespace LogicLink;

public static class HandyExtentions
{
    public static Vector3 Multiply(this Vector3 vector1, Vector3 vector2)
    {
        return new Vector3(vector1.x * vector2.x, vector1.y * vector2.y, vector1.z * vector2.z);
    }

    public static string Pluralise(this int number, string single, string postfix)
    {
        if (number == 1) return single;
        return single + postfix;
    }


    public static bool KeyDown(this Setting<KeyCode> setting)
    {
        return Input.GetKeyDown(setting);
    }

    public static bool KeyUp(this Setting<KeyCode> setting)
    {
        return Input.GetKeyUp(setting);
    }

    public static bool Key(this Setting<KeyCode> setting)
    {
        return Input.GetKey(setting);
    }


    public static ZeeplevelData ToBlueprint(this List<Block> blocks, string origin)
    {
        v15LevelJSON levelJSON = new();
        levelJSON.blox = [.. blocks.Select((block) => block.GetBlockProperties())];
        levelJSON.level = new($"LL - {origin}", "", "");

        ZeeplevelData levelData = new();
        levelData.json = levelJSON;
        levelData.isValid = true;
        return levelData;
    }

    public static List<BlockProperties> PasteIntoEditor(this ZeeplevelData blueprint, LEV_LevelEditorCentral central, bool reUID)
    {
        List<BlockProperties> list = ZeeplevelHandler.LoadIntoEditor(blueprint, central, reUID);
        Vector3 move = ToolkitUtils.BlocksAtCameraGridMovement(central, list);
        EditorTransformOperations.Move(central, list, move);
        return list;
    }



    public static string ToReadableString(this MoveMode moveMode)
    {
        return moveMode switch
        {
            MoveMode.Combined => "Combined",
            MoveMode.Strict => "Strict",
            MoveMode.LoosePosition => "Loose (Position)",
            MoveMode.LooseRotation => "Loose (Rotation)",
            _ => "Unkown"
        };
    }
}
