using LogicLink.Selection;
using UnityEngine;

namespace LogicLink;

public static class HandyExtentions
{
    public static Vector3 Multiply(this Vector3 vector1, Vector3 vector2)
    {
        return new Vector3(vector1.x * vector2.x, vector1.y * vector2.y, vector1.z * vector2.z);
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
