using LogicLink.Generator.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogicLink.Generator.Generators
{
    public static class JukeBoxGenerator
    {
        public static List<Block> GenerateJukeBox(MusicBlock.Instruments instrument, long[] notes, Vector3 offset = new())
        {
            List<Block> blocks = [];

            blocks.AddRange(RomGenerator.Generate(notes, out PassthroughBlock input, out PassthroughBlock output, new Vector3(-8, 0, 120) + offset));
            blocks.AddRange(GenerateSoundCard(instrument, out PassthroughBlock toRom, out PassthroughBlock fromRom, offset));

            toRom.Out.ConnectTo(input.In);
            output.Out.ConnectTo(fromRom.In);

            return blocks;
        }

        public static List<Block> GenerateSoundCard(MusicBlock.Instruments instrument, out PassthroughBlock toRom, out PassthroughBlock fromRom, Vector3 offset = new())
        {
            List<Block> blocks = [];

            PassthroughBlock input = new();
            blocks.Add(input);
            input.Position = offset;
            input.Rotation = Vector3.up * 270;

            PassthroughBlock output = new();
            blocks.Add(output);
            output.Position = new Vector3(-24, 0, -8) + offset;
            output.Rotation = Vector3.up * 180;

            ResettableTickerBlock timer = new();
            blocks.Add(timer);
            timer.Position = new Vector3(32, 0, 16) + offset;
            timer.Rotation = Vector3.up * 90;
            input.Out.ConnectTo(timer.Reset);

            CounterBlock enableTimer = new();
            blocks.Add(enableTimer);
            enableTimer.Position = new Vector3(16, 0, 16) + offset;
            enableTimer.Rotation = Vector3.up * 270;
            input.Out.ConnectTo(enableTimer.CountUp);

            TransistorBlock transistorTimer = new();
            blocks.Add(transistorTimer);
            transistorTimer.Position = new Vector3(-16, 0, 16) + offset;
            timer.Out.ConnectTo(transistorTimer.In);
            enableTimer.Out.ConnectTo(transistorTimer.Enable);

            MathBlock mathTimer = new();
            blocks.Add(mathTimer);
            mathTimer.Position = new Vector3(0, 0, 32) + offset;
            mathTimer.Rotation = Vector3.up * 90;
            mathTimer.Opperator = MathBlock.Opperators.Subtract;
            mathTimer.Num2Override = 1;
            transistorTimer.Out.ConnectTo(mathTimer.Num1);

            MathBlock nextNote = new();
            blocks.Add(nextNote);
            nextNote.Position = new Vector3(32, 0, 48) + offset;
            nextNote.Rotation = Vector3.up * 270;
            nextNote.Opperator = MathBlock.Opperators.Equal;
            mathTimer.Out.ConnectTo(nextNote.Num1);

            MathBlock endSong = new();
            blocks.Add(endSong);
            endSong.Position = new Vector3(32, 8, 48) + offset;
            endSong.Rotation = Vector3.up * 270;
            endSong.Opperator = MathBlock.Opperators.GreaterThan;
            mathTimer.Out.ConnectTo(endSong.Num1);

            DelayBlock delayBlock = new();
            blocks.Add(delayBlock);
            delayBlock.Position = new Vector3(-16, 8, 48) + offset;
            delayBlock.Rotation = Vector3.up * 270;
            endSong.Out.ConnectTo(delayBlock.In);
            delayBlock.Out.ConnectTo(output.In);

            CounterBlock indexCounter = new();
            blocks.Add(indexCounter);
            indexCounter.Position = new Vector3(-16, 0, 48) + offset;
            input.Out.ConnectTo(indexCounter.Reset);
            nextNote.Out.ConnectTo(indexCounter.CountUp);

            DelayBlock romDelay = new();
            blocks.Add(romDelay);
            romDelay.Position = new Vector3(-16, 0, 80) + offset;
            romDelay.Rotation = Vector3.up * 270;
            indexCounter.Out.ConnectTo(romDelay.In);

            toRom = new();
            blocks.Add(toRom);
            toRom.Position = new Vector3(-16, 0, 104) + offset;
            romDelay.Out.ConnectTo(toRom.In);

            fromRom = new();
            blocks.Add(fromRom);
            fromRom.Position = new Vector3(-8, 0, 96) + offset;
            fromRom.Rotation = Vector3.up * 180;

            TransistorBlock toTogglers = new();
            blocks.Add(toTogglers);
            toTogglers.Position = new Vector3(16, 0, 80);
            toTogglers.Rotation = Vector3.up * 90;
            fromRom.Out.ConnectTo(toTogglers.In);
            nextNote.Out.ConnectTo(toTogglers.Enable);

            MathBlock noteTime = new();
            blocks.Add(noteTime);
            noteTime.Position = new Vector3(0, 0, 64) + offset;
            noteTime.Rotation = Vector3.up * 90;
            noteTime.Opperator = MathBlock.Opperators.Divide;
            noteTime.Num2Override = 1L << 24;
            fromRom.Out.ConnectTo(noteTime.Num1);
            noteTime.Out.ConnectTo(nextNote.Num2);
            noteTime.Out.ConnectTo(endSong.Num2);

            Vector3 sliceOffset = new(32, 0, 48);
            ConnectionOut lastOutput = toTogglers.Out;
            for (int i = 0; i < 24; i++)
            {
                blocks.AddRange(GenerateSlice(sliceOffset, addDivide: i > 0, out ConnectionIn sliceInput, out ConnectionOut sliceOutput, out MusicBlock musicBlock));

                lastOutput.ConnectTo(sliceInput);
                if (sliceOutput != null) lastOutput = sliceOutput;

                musicBlock.Note = i;
                musicBlock.Instrument = instrument;
                musicBlock.Position = new Vector3(32, 0, 0) + offset;

                sliceOffset += Vector3.right * 16;
            }

            return blocks;
        }

        public static List<Block> GenerateSlice(Vector3 offset, bool addDivide, out ConnectionIn input, out ConnectionOut output, out MusicBlock musicBlock)
        {
            List<Block> blocks = [];
            output = null;

            musicBlock = new();
            blocks.Add(musicBlock);
            musicBlock.Position = new Vector3(0, 0, 128) + offset;

            MathBlock modulo = new();
            blocks.Add(modulo);
            modulo.Position = new Vector3(0, 0, 48) + offset;
            modulo.Opperator = MathBlock.Opperators.Modulo;
            modulo.Num2Override = 2;
            input = modulo.Num1;

            DigitizerBlock digitizer = new();
            blocks.Add(digitizer);
            digitizer.Position = new Vector3(0, 0, 96) + offset;
            digitizer.Invert = true;
            modulo.Out.ConnectTo(digitizer.In);

            TogglerBlock toggler = new();
            blocks.Add(toggler);
            toggler.Position = new Vector3(0, 0, 112) + offset;
            digitizer.Out.ConnectTo(toggler.In);
            toggler.BlockIds[0] = musicBlock.Id;

            if (addDivide)
            {
                MathBlock divide = new();
                blocks.Add(divide);
                divide.Position = offset;
                divide.Opperator = MathBlock.Opperators.Divide;
                divide.Num2Override = 2;
                divide.Out.ConnectTo(modulo.Num1);
                input = divide.Num1;
                output = divide.Out;
            }

            return blocks;
        }
    }
}
