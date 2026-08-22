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
    public static class TogglerGenerator
    {
        public static List<Block> CreateTogglers(string[] ids, Vector3 offset = new()) => CreateTogglers(ids, out _, offset);

        public static List<Block> CreateTogglers(string[] ids, out ConnectionIn inputConnection, Vector3 offset = new())
        {
            string[][] idsChunks = [.. ids.Chunk(TogglerBlock.MaxIds).Select<IEnumerable<string>, string[]>((segment) => [.. segment])];
            List<Block> blocks = [];

            if (idsChunks.Length == 1) return [CreateToggler(idsChunks[0], out inputConnection, offset)];

            PassthroughBlock input = new();
            blocks.Add(input);
            inputConnection = input.In;
            input.Position = Vector3.zero + offset;

            Vector3 currentOffset = new Vector3(0, 0, 16) + offset;
            for (int i = 0; i < idsChunks.Length; i++)
            {
                string[] chunk = idsChunks[i];

                TogglerBlock toggler = CreateToggler(chunk, out ConnectionIn togglerIn, currentOffset);
                blocks.Add(toggler);
                input.Out.ConnectTo(togglerIn);

                currentOffset += Vector3.left * 4;
            }

            return blocks;
        }

        private static TogglerBlock CreateToggler(string[] ids, out ConnectionIn inputConnection, Vector3 offset = new())
        {
            TogglerBlock toggler = new();
            inputConnection = toggler.In;
            toggler.Position = offset;

            for (int j = 0; j < Math.Min(ids.Length, toggler.BlockIds.Length); j++)
            {
                string id = ids[j];
                toggler.BlockIds[j] = id;
            }

            return toggler;
        }
    }
}
