using Imui.Controls;
using Imui.Core;
using LogicLink.Generator;
using LogicLink.Generator.Generators.VideoToSigns;
using LogicLink.LogicV2.Patches;
using LogicLink.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using ZeepSDK.LevelEditor;
using ZeepSDK.Messaging;
using ZeepSDK.UI;

namespace LogicLink.LogicV2
{
    public class VideoGeneratorDrawer : IZeepGUIDrawer
    {
        private bool _windowOpen = false;

        private Setting<string> PathSetting = new("Generators", "VideoFilePath", String.Empty, "The video file to convert", hidden: true);
        private List<string> ErrorMessages = [];
        private string ConvertErrorMessage = String.Empty;

        private Setting<string> TicksPerFrameStr = new("Generators", "Ticks Per Frame", 20.ToString(), "How many ticks are between frames", hidden: true);
        private Setting<string> WidthStr = new("Generators", "Video Width", (VideoConverter.signWidth * 3).ToString(), "Width of the video in pixels", hidden: true);
        private Setting<string> StartTimeStr = new("Generators", "Video Start Time", String.Empty, "Where the converters starts", hidden: true);
        private Setting<string> EndTimeStr = new("Generators", "Video End Time", String.Empty, "Where the converter ends", hidden: true);

        private int TicksPerFrame;
        private int Width;
        private double StartTime;
        private double EndTime;

        private string FileName = String.Empty;

        private Thread ConvertThread;
        private int CurrentFrame;
        private int TotalFrames;
        private int TotalBlocks;
        private bool Abort;
        private List<Block> Blocks;

        private ConvertingStage ConvertingVideoState = ConvertingStage.NoFile;


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
            if (!_windowOpen || !gui.BeginWindow("Video Generator", ref _windowOpen, (width, height))) return;

            if (ConvertingVideoState == ConvertingStage.NoFile) DrawFileSelection(gui);
            else if (ConvertingVideoState == ConvertingStage.ConvertingVideo) DrawConvertingStage(gui);
            else if (ConvertingVideoState == ConvertingStage.ReadyToLoad) DrawReadyToLoadStage(gui);

            gui.EndWindow();
        }


        private void DrawFileSelection(ImGui gui)
        {
            gui.Text("File Path:", new ImTextSettings(16));
            PathSetting.Value = gui.TextEdit(PathSetting);

            //using (gui.Horizontal())
            {
                using (gui.Vertical())
                {
                    gui.Text("Ticks Per Frame");
                    TicksPerFrameStr.Value = gui.TextEdit(TicksPerFrameStr);
                }
                using (gui.Vertical())
                {
                    gui.Text("Pixel Width");
                    WidthStr.Value = gui.TextEdit(WidthStr);
                    gui.TooltipAtLastControl($"Every sign has a {VideoConverter.signWidth} pixel width");
                }
            }

            //using (gui.Horizontal())
            {
                using (gui.Vertical())
                {
                    gui.Text("Start Time");
                    StartTimeStr.Value = gui.TextEdit(StartTimeStr);
                    gui.TooltipAtLastControl("Start Time in seconds, leave empty to start at the beginning of the video");

                }
                using (gui.Vertical())
                {
                    gui.Text("End Time");
                    EndTimeStr.Value = gui.TextEdit(EndTimeStr);
                    gui.TooltipAtLastControl("End Time in seconds, leave empty to stop at the end of the video");
                }
            }

            ParseSettings();
            foreach (string message in ErrorMessages)
            {
                gui.Text(message, new ImTextSettings(12), new Color32(200, 0, 0, 255));
            }

            using (gui.Vertical(30, 30)) { }
            if (gui.Button("Convert Video"))
            {
                Plugin.Logger.LogInfo($"Video Generator - Converting '{PathSetting}'");
                ConvertVideo(PathSetting);
            }
            if (!string.IsNullOrWhiteSpace(ConvertErrorMessage)) gui.Text(ConvertErrorMessage, new ImTextSettings(12), new Color32(200, 0, 0, 255));

            using (gui.Vertical(5, 5)) { }
            gui.Text("Powered by Murrl's and Tera's png to sign converter!", new ImTextSettings(15));
        }

        private void ParseSettings()
        {
            ErrorMessages.Clear();
            if (!int.TryParse(TicksPerFrameStr, out TicksPerFrame)) ErrorMessages.Add("Could not parse Ticks Per Frame!");
            if (!int.TryParse(WidthStr, out Width)) ErrorMessages.Add("Could not parse Width!");
            if (!double.TryParse(StartTimeStr, out StartTime))
            {
                if (string.IsNullOrWhiteSpace(StartTimeStr)) StartTime = 0;
                else ErrorMessages.Add("Could not parse Start Time!");
            }
            if (!double.TryParse(EndTimeStr, out EndTime))
            {
                if (string.IsNullOrWhiteSpace(EndTimeStr)) EndTime = 0;
                else ErrorMessages.Add("Could not parse End Time!");
            }
        }


        private void DrawConvertingStage(ImGui gui)
        {
            gui.Text($"Converting '{FileName}'...", true);
            gui.Text($"Frame {CurrentFrame}/{TotalFrames}");
            gui.Text($"Blocks {TotalBlocks}");
            using (gui.Vertical(30, 30)) { }
            gui.Text("You can close this window in the meantime", new ImTextSettings(12));
            if (gui.Button(!Abort ? "Cancel" : "Canceling..."))
            {
                Abort = true;
            }
        }


        private void DrawReadyToLoadStage(ImGui gui)
        {
            gui.Text($"Converted '{FileName}'", true);
            gui.Text($"Generated {TotalFrames} Frames - {TotalBlocks} Blocks");
            using (gui.Vertical(30, 30)) { }
            gui.Text("Expect a lag spike when pasting", new ImTextSettings(12));
            using (gui.Horizontal())
            {
                if (gui.Button("Paste"))
                {
                    _windowOpen = false;
                    LogicScript_Door_VisualizeToggler.ShouldSkip = true;
                    Blocks.ToBlueprint("VideoConverter").PasteIntoEditor(Plugin.Central, true);
                    Blocks.Clear();
                    ConvertingVideoState = ConvertingStage.NoFile;
                }
                if (gui.Button("Unload"))
                {
                    Blocks.Clear();
                    ConvertingVideoState = ConvertingStage.NoFile;
                }
            }
        }



        private void ConvertVideo(string path)
        {
            if (!File.Exists(path))
            {
                ConvertErrorMessage = "File not found!";
                Plugin.Logger.LogWarning("File Not Found!");
                return;
            }

            FileName = Path.GetFileName(path);
            ConvertingVideoState = ConvertingStage.ConvertingVideo;
            ConvertErrorMessage = "";

            ConvertThread = new(new ThreadStart(() => ConvertVideoThread(path)));
            ConvertThread.Start();
        }

        private void ConvertVideoThread(string path)
        {
            CurrentFrame = 0;
            TotalFrames = 0;
            TotalBlocks = 0;
            Abort = false;
            try
            {
                TimeSpan startTime = Mathf.Approximately((float)StartTime, 0) ? new(0) : TimeSpan.FromSeconds(StartTime);
                TimeSpan endTime = Mathf.Approximately((float)EndTime, 0) ? new(0) : TimeSpan.FromSeconds(EndTime);

                Blocks = VideoConverter.ConvertVideo(path, TicksPerFrame, Width, startTime, endTime, ref CurrentFrame, ref TotalFrames, ref TotalBlocks, ref Abort);
                if (Blocks == null || Abort)
                {
                    ConvertingVideoState = ConvertingStage.NoFile;
                    return;
                }

                MainThreadScheduler.RunAction(() => MessengerApi.Log("Finished Converting Video"));
                ConvertingVideoState = ConvertingStage.ReadyToLoad;
            }
            catch (Exception ex)
            {
                ConvertErrorMessage = "Something went wrong while trying to convert video! " + ex.Message;
            }
        }



        private enum ConvertingStage
        {
            NoFile,
            ConvertingVideo,
            ReadyToLoad,
        }
    }
}
