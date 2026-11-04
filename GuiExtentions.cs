using Imui.Controls;
using Imui.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink
{
    public static class GuiExtentions
    {
        public static bool FitButton(this ImGui gui, string label, RectUtils.Position position = RectUtils.Position.Center)
        {
            ImRect buttonContainer = gui.AddSingleRowRect((gui.Layout.GetAvailableWidth(), gui.GetTextLineHeight() + gui.Style.Layout.InnerSpacing * 2));
            ImRect button = buttonContainer.GetRect(RectUtils.GetTextWidth(label, gui), buttonContainer.H - gui.Style.Layout.InnerSpacing, position);

            return gui.Button(label, button);
        }
    }
}
