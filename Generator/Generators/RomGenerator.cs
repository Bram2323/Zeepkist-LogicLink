using LogicLink.Generator.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ZeepSDK.Extensions;

namespace LogicLink.Generator.Generators
{
    public static class RomGenerator
    {
        public static List<Block> Generate(long[] numbers, Vector3 offset = new()) => Generate(numbers, out _, out _, offset);

        public static List<Block> Generate(long[] numbers, out PassthroughBlock input, out PassthroughBlock output, Vector3 offset = new())
        {
            long[][] segmentNumbers = [.. numbers.Chunk(4).Select<IEnumerable<long>, long[]>((segment) => [.. segment])];
            List<Block> blocks = [];

            input = new();
            output = new();
            blocks.AddRange([input, output]);

            input.Position = Vector3.zero + offset;
            output.Position = new Vector3(8, 0, -8) + offset;
            output.Rotation = new Vector3(0, 90, 0);

            MathBlock lastSegmentInput = null;
            MathBlock lastSegmentOutput = null;
            Vector3 currentOffset = new Vector3(0, 0, 16) + offset;

            for (int i = 0; i < segmentNumbers.Length; i++)
            {
                bool first = i == 0;
                blocks.AddRange(GenerateSegment(segmentNumbers[i], first, out MathBlock segmentInput, out MathBlock segmentOutput, currentOffset));
                if (first)
                {
                    input.Out.ConnectTo(segmentInput.Num1);
                    segmentOutput.Out.ConnectTo(output.In);
                }
                else
                {
                    lastSegmentInput.Out.ConnectTo(segmentInput.Num1);
                    segmentOutput.Out.ConnectTo(lastSegmentOutput.Num1);
                }

                lastSegmentInput = segmentInput;
                lastSegmentOutput = segmentOutput;
                currentOffset += Vector3.left * 16;
            }

            return blocks;
        }

        private static List<Block> GenerateSegment(long[] numbers, bool firstSegment, out MathBlock input, out MathBlock output, Vector3 offset)
        {
            List<Block> blocks = [];

            MuxBlock muxBlock = new();
            input = new();
            output = new();
            blocks.AddRange([muxBlock, input, output]);

            muxBlock.Position = new Vector3(0, 0, 16) + offset;
            input.Position = new Vector3(0, 8, 16) + offset;
            output.Position = new Vector3(0, 0, 64) + offset;

            input.Out.ConnectTo(muxBlock.Selector);
            muxBlock.Out.ConnectTo(output.Num2);

            if (firstSegment) input.Num2Override = 1;
            else
            {
                input.Opperator = MathBlock.Opperators.Subtract;
                input.Num2Override = 4;
            }

            for (int i = 0; i < numbers.Length; i++)
            {
                GeneratorBlock generatorBlock = new();
                blocks.Add(generatorBlock);

                generatorBlock.Position = new Vector3(0, i * 2, 0) + offset;
                generatorBlock.Value = numbers[i];

                ConnectionIn muxIn = muxBlock.Inputs[i];
                generatorBlock.Out.ConnectTo(muxIn);
            }

            return blocks;
        }
    }
}
