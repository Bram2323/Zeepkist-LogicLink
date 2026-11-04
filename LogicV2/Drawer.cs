using Imui.Controls;
using Imui.Core;
using LogicLink.LogicV2.Drawers;
using LogicLink.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ZeepSDK.LevelEditor;
using ZeepSDK.UI;

namespace LogicLink.LogicV2
{
    public abstract class Drawer : IZeepGUIDrawer
    {
        private bool _windowOpen = false;

        public abstract int Height { get; }
        public abstract int Width { get; }
        public abstract string Title { get; }


        public void Toggle()
        {
            if (CanOpen()) Open();
            else if (IsOpen()) Close();
        }


        public void Open()
        {
            if (!CanOpen()) return;
            _windowOpen = true;
        }

        public void Close()
        {
            _windowOpen = false;
        }

        public bool CanOpen()
        {
            return !_windowOpen && Plugin.Central != null && LevelEditorApi.IsInLevelEditor;
        }

        public bool IsOpen()
        {
            return _windowOpen;
        }


        public void OnZeepGUI(ImGui gui)
        {
            if (!IsOpen() || !gui.BeginWindow(Title, ref _windowOpen, (Width, Height))) return;
            DrawGUI(gui);
            gui.EndWindow();
        }

        public abstract void DrawGUI(ImGui gui);
    }
}
