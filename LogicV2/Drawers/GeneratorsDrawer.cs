using Imui.Controls;
using Imui.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.LogicV2.Drawers
{
    public class GeneratorsDrawer : Drawer
    {
        public override int Height => 700;

        public override int Width => 500;

        public override string Title => "Generators";


        public Drawer[] Generators;


        public override void DrawGUI(ImGui gui)
        {
            foreach (Drawer drawer in Generators)
            {
                if (gui.Button(drawer.Title))
                {
                    drawer.Open();
                    Close();
                }
            }
        }
    }
}
