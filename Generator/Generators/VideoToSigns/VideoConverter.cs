using LogicLink.Generator.Blocks;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LogicLink.Generator.Generators.VideoToSigns
{
    public static class VideoConverter
    {
        public const int signWidth = 27;
        private const string signPrefix = "<font=\"Roboto-Bold SDF\"><indent=-86%><size=20><cspace=-4><line-height=18%>";
        private const string signPixel = ".";
        private const string transparentColor = "<#0000>";

        public static readonly string OutputDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(Plugin)).Location), "VideoConverter");
        public static readonly string OutputFile = Path.Combine(OutputDirectory, "frame.jpeg");


        public static List<Block> ConvertVideo(string path, int ticksPerFrame, int width, TimeSpan start, TimeSpan end,
            ref int currentFrame, ref int totalFrames, ref int totalBlocks, ref bool abort)
        {
            List<Block> blocks = [];

            List<string[]> framesText = GetVideo(path, ticksPerFrame, width, start, end, ref currentFrame, ref totalFrames, ref totalBlocks, ref abort);
            if (framesText == null || abort) return null;

            blocks.AddRange(CreateTimer(out ConnectionOut timerOutput, ticksPerFrame, framesText.Count));

            int frameNum = 0;
            Vector3 currentOffset = new(16, 0, 48);
            foreach (string[] frame in framesText)
            {
                List<Block> frameBlocks = GetBlocksForFrame(frame, Vector3.back * 16);
                blocks.AddRange(frameBlocks);
                blocks.AddRange(CreateTogglerSlice(frameBlocks, timerOutput, frameNum, currentOffset));
                currentOffset += Vector3.forward * 16;

                frameNum++;
            }

            totalBlocks = blocks.Count;
            return blocks;
        }


        public static List<string[]> GetVideo(string path, int ticksPerFrame, int width, TimeSpan start, TimeSpan end,
            ref int currentFrame, ref int totalFrames, ref int totalBlocks, ref bool abort)
        {
            ClearOutputDirectory();
            List<string[]> frames = [];
            if (ticksPerFrame <= 0) throw new Exception("Ticks Per Frame needs to be more or equal to 1!");
            double timePerFrames = ticksPerFrame / 90.0;

            using (Engine engine = new())
            {
                MediaFile video = new() { Filename = path };
                engine.GetMetadata(video);
                if (end.Ticks == 0) end = video.Metadata.Duration;

                totalFrames = 0;
                TimeSpan searchTime = start;
                while (searchTime < video.Metadata.Duration && searchTime < end)
                {
                    searchTime += TimeSpan.FromSeconds(timePerFrames);
                    totalFrames++;
                }

                if (totalFrames == 0) throw new Exception("No frames to convert! Choose different settings!");

                currentFrame = 1;
                TimeSpan currentTime = start;
                while (currentTime < video.Metadata.Duration && currentTime < end && !abort)
                {
                    ConversionOptions options = new() { Seek = currentTime };
                    MediaFile output = new() { Filename = OutputFile };
                    engine.GetThumbnail(video, output, options);
                    Texture2D texture = MainThreadScheduler.RunFunc(() => GetTextureForCurrentFrame(width));
                    string[] text = GetTextForFrame(texture);
                    frames.Add(text);
                    currentTime += TimeSpan.FromSeconds(timePerFrames);

                    totalBlocks += text.Length + 1 + (text.Length - 1) / 8 + 1;
                    currentFrame++;
                }
            }

            if (abort)
            {
                Plugin.Logger.LogInfo($"Aborted converting video!");
                return null;
            }

            Plugin.Logger.LogInfo($"Converted video to {frames.Count} frames!");
            return frames;
        }

        private static Texture2D GetTextureForCurrentFrame(int width)
        {
            byte[] data = File.ReadAllBytes(OutputFile);
            Texture2D texture = new(2, 2);
            texture.LoadImage(data);

            if (width > texture.width) return texture;

            float ratio = (float)width / texture.width;
            int height = Mathf.RoundToInt(texture.height * ratio);

            TextureScale.Bilinear(texture, width, height);

            return texture;
        }

        private static List<Block> GetBlocksForFrame(string[] frame, Vector3 offset = new())
        {
            List<Block> blocks = [];

            Vector3 currentOffset = Vector3.zero;
            foreach (string text in frame)
            {
                SignBlock sign = new();
                blocks.Add(sign);
                sign.Position = currentOffset + offset;
                sign.Text = text;

                currentOffset += Vector3.right * 40;
            }

            return blocks;
        }

        private static string[] GetTextForFrame(Texture2D texture)
        {
            int signs = (texture.width - 1) / signWidth + 1;

            string[] text = new string[signs];
            string[] lastColor = new string[signs];
            for (int i = 0; i < text.Length; i++) text[i] = signPrefix;

            for (int y = texture.height - 1; y >= 0; y--)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    int sign = x / signWidth;

                    Color32 color = texture.GetPixel(x, y);
                    string hex = ColorToHex(color);
                    if (lastColor[sign] != hex) text[sign] += hex;
                    text[sign] += signPixel;
                    lastColor[sign] = hex;

                    if (x == texture.width - 1 && texture.width % signWidth != 0)
                    {
                        text[sign] += transparentColor;
                        lastColor[sign] = transparentColor;

                        for (int i = 0; i < signWidth - texture.width % signWidth; i++)
                        {
                            text[sign] += signPixel;
                        }
                    }
                }
            }

            return text;
        }


        public static List<Block> CreateTimer(out ConnectionOut output, int ticksPerFrame, int frames)
        {
            List<Block> blocks = [];

            ResettableTickerBlock ticker = new();
            blocks.Add(ticker);
            ticker.Rotation = new(0, 180, 0);

            MathBlock divide = new();
            blocks.Add(divide);
            divide.Position = new(0, 0, 16);
            divide.Opperator = MathBlock.Opperators.Divide;
            divide.Num2Override = ticksPerFrame;

            MathBlock reset = new();
            blocks.Add(reset);
            reset.Rotation = new(0, 180, 0);
            reset.Position = new(-16, 0, 48);
            reset.Opperator = MathBlock.Opperators.GreaterThanOrEqual;
            reset.Num2Override = ticksPerFrame * frames - 1;

            DelayBlock delay = new();
            blocks.Add(delay);
            delay.Rotation = new(0, 90, 0);
            delay.Position = new(-16, 0, 0);

            ticker.Out.ConnectTo(divide.Num1);
            ticker.Out.ConnectTo(reset.Num1);
            reset.Out.ConnectTo(delay.In);
            delay.Out.ConnectTo(ticker.Reset);

            output = divide.Out;
            return blocks;
        }

        private static List<Block> CreateTogglerSlice(List<Block> signs, ConnectionOut timerOutput, int frame, Vector3 offset)
        {
            List<Block> blocks = [];

            blocks.AddRange(TogglerGenerator.CreateTogglers([.. signs.Select((sign) => sign.Id)], out ConnectionIn input, offset + Vector3.right * 48));
            MathBlock mathBlock = new();
            blocks.Add(mathBlock);
            mathBlock.Rotation = new(0, 90, 0);
            mathBlock.Position = offset;
            mathBlock.Opperator = MathBlock.Opperators.NotEqual;
            mathBlock.Num2Override = frame;

            timerOutput.ConnectTo(mathBlock.Num1);
            mathBlock.Out.ConnectTo(input);

            return blocks;
        }



        private static Color32 SnapToHex3(Color32 color)
        {
            (byte r, byte g, byte b, byte a) = (color.r, color.g, color.b, color.a);
            a = Round(a);
            if (a == 0) return new();
            return new(Round(r), Round(g), Round(b), a);
        }

        private static string ColorToHex(Color32 color)
        {
            color = SnapToHex3(color);

            string hex = "<#";
            hex += ByteToHex(color.r);
            hex += ByteToHex(color.g);
            hex += ByteToHex(color.b);
            if (color.a != 255) hex += ByteToHex(color.a);
            hex += ">";

            return hex;
        }

        private static char ByteToHex(byte num)
        {
            return num.ToString("X")[0];
        }

        private static byte Round(byte num)
        {
            return (byte)(Mathf.RoundToInt(num / 17) * 17);
        }



        private static void ClearOutputDirectory()
        {
            if (!Directory.Exists(OutputDirectory)) Directory.CreateDirectory(OutputDirectory);
            if (File.Exists(OutputFile)) File.Delete(OutputFile);
        }
    }
}
