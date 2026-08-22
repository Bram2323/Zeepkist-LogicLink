using Imui.Controls;
using Imui.Core;
using LogicLink.Generator;
using LogicLink.Generator.Blocks;
using LogicLink.Generator.Generators;
using LogicLink.Settings;
using System.Collections.Generic;
using System.IO;
using Toolkist;
using Toolkist.EditorOperations;
using UnityEngine;
using ZeepSDK.LevelEditor;
using ZeepSDK.UI;

namespace LogicLink.LogicV2
{
    public class RomGeneratorDrawer : IZeepGUIDrawer
    {
        private bool _windowOpen = false;

        private Setting<string> PathSetting = new("Generators", "RomFilePath", "", "The file that the Rom Generator will use", hidden: true);
        private Setting<string> SeperatorSetting = new("Generators", "RomSeperator", ",", "What the Rom Generator will use to seperate the numbers", hidden: true);
        private string ReadErrorMessage = "";
        private string ConvertWarningMessage = "";
        private string ConvertMessage = "";

        private bool LoadedFile = false;
        private string FileName = "";
        private long[] Numbers;


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
            int width = 700;
            int height = 500;
            if (!_windowOpen || !gui.BeginWindow("Rom Generator", ref _windowOpen, (width, height))) return;

            DrawFileSelection(gui);
            DrawNumberEditor(gui);

            gui.EndWindow();
        }



        private void DrawFileSelection(ImGui gui)
        {
            gui.Text("File Path:", new ImTextSettings(16));
            PathSetting.Value = gui.TextEdit(PathSetting);
            gui.Text("Seperator:", new ImTextSettings(16));
            SeperatorSetting.Value = gui.TextEdit(SeperatorSetting);
            gui.TooltipAtLastControl("Fill in what seperates the numbers (new lines get seperated automatically)");
            if (gui.Button("Open File", (150, 30)))
            {
                Plugin.Logger.LogInfo($"Rom Generator - Opening '{PathSetting}'");
                ClearWarnings();
                LoadedFile = OpenFile(PathSetting);
            }
            if (!string.IsNullOrWhiteSpace(ReadErrorMessage)) gui.Text(ReadErrorMessage, new ImTextSettings(12), new Color32(200, 0, 0, 255));
            if (!string.IsNullOrWhiteSpace(ConvertWarningMessage)) gui.Text(ConvertWarningMessage, new ImTextSettings(12), new Color32(240, 240, 0, 255));
            if (!string.IsNullOrWhiteSpace(ConvertMessage)) gui.Text(ConvertMessage, new ImTextSettings(12));
        }


        private void DrawNumberEditor(ImGui gui)
        {
            if (!LoadedFile) return;

            using (gui.Vertical(0, 30)) { }

            gui.Text($"Current file: '{FileName}'");
            gui.Text($"{Numbers.Length} {Numbers.Length.Pluralise("number", "s")}");

            using (gui.Vertical(0, 30)) { }

            if (gui.Button("Generate Rom", (200, 30)))
            {
                GenerateRom();
                Close();
            }
        }


        private void GenerateRom()
        {
            List<Block> blocks = RomGenerator.Generate(Numbers);
            ZeeplevelData blueprint = blocks.ToBlueprint("RomGenerator");
            blueprint.PasteIntoEditor(Plugin.Central, false);
        }


        private void ClearWarnings()
        {
            ReadErrorMessage = "";
            ConvertWarningMessage = "";
            ConvertMessage = "";

            GeneratorBlock genBlock = new();
        }

        private bool OpenFile(string path)
        {
            if (!File.Exists(path))
            {
                ReadErrorMessage = "File not found!";
                Plugin.Logger.LogWarning("File Not Found!");
                return false;
            }

            FileName = Path.GetFileName(path);
            string[] fileContent = File.ReadAllLines(path);
            List<string> numbers = [];
            foreach (string line in fileContent)
            {
                numbers.AddRange(line.Split(SeperatorSetting));
            }
            return ConvertNumbers([.. numbers]);
        }

        private bool ConvertNumbers(string[] numberStrings)
        {
            List<long> numbers = [];

            int fails = 0;
            foreach (string numberString in numberStrings)
            {
                if (long.TryParse(numberString, out long number)) numbers.Add(number);
                else fails++;
            }

            if (numbers.Count == 0 && numberStrings.Length > 0)
            {
                ReadErrorMessage = $"All {numberStrings.Length} {numberStrings.Length.Pluralise("number", "s")} failed to parse!";
                return false;
            }
            else if (fails > 0)
            {
                ConvertWarningMessage = $"{fails} out of {numberStrings.Length} numbers failed to parse!";
            }
            else
            {
                ConvertMessage = $"Loaded {numbers.Count} {numbers.Count.Pluralise("number", "s")}!";
            }

            Numbers = [.. numbers];
            return true;
        }
    }
}
