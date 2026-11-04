using Imui.Controls;
using Imui.Core;
using LogicLink.Generator;
using LogicLink.Generator.Blocks;
using LogicLink.Generator.Generators;
using LogicLink.Settings;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.IO;
using Toolkist;
using UnityEngine;
using ZeepSDK.LevelEditor;
using ZeepSDK.UI;

namespace LogicLink.LogicV2.Drawers
{
    public class MusicGeneratorDrawer : Drawer
    {
        public override int Height => 700;
        public override int Width => 700;
        public override string Title => "Music Generator";

        public const int MinNote = 48;
        public const int MaxNote = MinNote + 23;
        public const int NoteRange = MaxNote - MinNote + 1;

        private Setting<string> PathSetting = new("Generators", "MusicFilePath", "", "The file that the Music Generator will use", hidden: true);

        private string ReadErrorMessage = "";
        private string ConvertWarningMessage = "";
        private string ConvertMessage = "";

        private bool LoadedFile = false;
        private string FileName = "";
        private MidiFile Midi;

        public override void DrawGUI(ImGui gui)
        {
            if (!LoadedFile) DrawFileSelection(gui);
            else DrawNoteEditor(gui);
        }


        private void DrawFileSelection(ImGui gui)
        {
            gui.Text("File Path:", new ImTextSettings(16));
            PathSetting.Value = gui.TextEdit(PathSetting);

            if (gui.FitButton("Open File"))
            {
                Plugin.Logger.LogInfo($"Music Generator - Opening '{PathSetting}'");
                ClearWarnings();
                LoadedFile = OpenFile(PathSetting);
            }
            if (!string.IsNullOrWhiteSpace(ReadErrorMessage)) gui.Text(ReadErrorMessage, new ImTextSettings(12), new Color32(200, 0, 0, 255));
            if (!string.IsNullOrWhiteSpace(ConvertWarningMessage)) gui.Text(ConvertWarningMessage, new ImTextSettings(12), new Color32(240, 240, 0, 255));
            if (!string.IsNullOrWhiteSpace(ConvertMessage)) gui.Text(ConvertMessage, new ImTextSettings(12));
        }

        private void DrawNoteEditor(ImGui gui)
        {
            gui.Text($"Current file: '{FileName}'");
            gui.AddSpacing();
            if (gui.FitButton("Generate JukeBox"))
            {
                GenerateJukeBox();
                Close();
            }
            if (gui.FitButton("Unload File"))
            {
                Midi = null;
                FileName = "";
                LoadedFile = false;
            }
        }


        private void ClearWarnings()
        {
            ReadErrorMessage = "";
            ConvertWarningMessage = "";
            ConvertMessage = "";
        }


        private void GenerateJukeBox()
        {
            long[] numbers = ConvertFile(Midi);
            List<Block> blocks = JukeBoxGenerator.GenerateJukeBox(MusicBlock.Instruments.Piano, numbers);
            ZeeplevelData blueprint = blocks.ToBlueprint("MusicGenerator");
            blueprint.PasteIntoEditor(Plugin.Central, false);
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

            try
            {
                FileStream stream = new(path, FileMode.Open);
                Midi = MidiFile.Read(stream);
            }
            catch
            {
                ConvertWarningMessage = "Something went wrong while converting file!";
                Plugin.Logger.LogWarning("Something went wrong while converting file!");
                return false;
            }

            return true;
        }

        private long[] ConvertFile(MidiFile midi)
        {
            TempoMap tempoMap = midi.GetTempoMap();
            ICollection<Note> notes = midi.GetNotes();

            List<long> numbers = [];

            long currentTicks = 0;
            long currentNotes = 0;

            foreach (Note note in notes)
            {
                MetricTimeSpan timeSpan = (MetricTimeSpan)note.TimeAs(TimeSpanType.Metric, tempoMap);
                long ticks = ConvertToTicks(timeSpan.TotalMicroseconds, currentTicks);

                int actualNote = note.NoteNumber;
                int noteNum = ConvertNote(actualNote);

                if (ticks > currentTicks)
                {
                    if (ticks - currentTicks == 1) ticks++;
                    numbers.Add((currentTicks << NoteRange) | currentNotes);
                    currentTicks = ticks;
                    currentNotes = 0;
                }

                if (actualNote > 1)
                {
                    currentNotes |= ((long)1) << noteNum;
                }
            }

            numbers.Add((currentTicks << NoteRange) | currentNotes);

            MetricTimeSpan endOfSong = (MetricTimeSpan)midi.GetDuration(TimeSpanType.Metric);
            numbers.Add((ConvertToTicks(endOfSong.TotalMicroseconds, currentTicks) << NoteRange) | 0);

            return [.. numbers];
        }

        public static long ConvertToTicks(long microseconds, long currentTicks)
        {
            long ticks = (microseconds * 90) / 1000000;
            if (currentTicks + 1 == ticks) ticks++;
            return ticks;
        }
        public static int ConvertNote(int noteNumber)
        {
            while (noteNumber < MinNote)
            {
                noteNumber += 12;
            }

            while (noteNumber > MaxNote)
            {
                noteNumber -= 12;
            }

            return noteNumber - MinNote;
        }
    }
}
