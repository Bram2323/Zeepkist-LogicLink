using BepInEx.Configuration;
using Imui.Controls;
using Imui.Core;
using LogicLink.Generator;
using LogicLink.LogicV1.Selection;
using LogicLink.Settings;
using System.Collections.Generic;
using System.Linq;
using Toolkist;
using Toolkist.EditorOperations;
using UnityEngine;

namespace LogicLink;

public static class RectUtils
{
    public static float Margin = 8f;


    public static ImRect SplitTop(this ImRect rect, float height, out ImRect bottom)
    {
        ImRect top = new(rect.X, rect.Top - height, rect.W, height);
        bottom = new(rect);
        bottom.H -= height;
        return top;
    }

    public static ImRect SplitBottom(this ImRect rect, float height, out ImRect top)
    {
        ImRect bottom = new(rect.X, rect.Y, rect.W, height);
        top = new(rect);
        top.Y += height;
        top.H -= height;
        return bottom;
    }

    public static ImRect splitLeft(this ImRect rect, float width, out ImRect right)
    {
        ImRect left = new(rect.X, rect.Y, width, rect.H);
        right = new(rect);
        right.X += width;
        right.W -= width;
        return left;
    }

    public static ImRect SplitRight(this ImRect rect, float width, out ImRect left)
    {
        ImRect right = new(rect.Right - width, rect.Y, width, rect.H);
        left = new(rect);
        left.W -= width;
        return right;
    }


    public static ImRect GetRect(this ImRect rect, float width, float height, Position position)
    {
        Position horizontal = position & Position.HMask;
        Position vertical = position & Position.VMask;

        float x = horizontal switch
        {
            Position.Right => rect.Right - width,
            Position.HMiddle => rect.X + (rect.W - width) / 2,
            _ => rect.X
        };
        float y = vertical switch
        {
            Position.Top => rect.Top - height,
            Position.VMiddle => rect.Y + (rect.H - height) / 2,
            _ => rect.Y
        };

        return new(x, y, width, height);
    }


    public static float GetTextWidth(string text, ImGui gui)
    {
        float width = gui.MeasureTextSize(text).x;
        width += gui.Style.Layout.InnerSpacing * 2 + Margin;
        return width;
    }


    public enum Position
    {
        Left = 0b00,
        HMiddle = 0b01,
        Right = 0b10,
        HMask = 0b11,
        Top = 0b0000,
        VMiddle = 0b0100,
        Bottom = 0b1000,
        VMask = 0b1100,

        TopLeft = Top | Left,
        TopMiddle = Top | HMiddle,
        TopRight = Top | Right,
        BottomLeft = Bottom | Left,
        BottomMiddle = Bottom | HMiddle,
        BottomRight = Bottom | Right,
        MiddleLeft = VMiddle | Left,
        Center = VMiddle | HMiddle,
        MiddleRight = VMiddle | Right,
    }
}
