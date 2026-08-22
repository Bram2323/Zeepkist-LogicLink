using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogicLink.Generator.Blocks
{
    public class TogglerBlock : LogicBlock
    {
        private const string referenceJson = "{\"i\":2417,\"u\":\"7vUl2FHQ4Eac-832417\",\"p\":{\"x\":-16,\"y\":0,\"z\":-416},\"r\":{\"x\":0,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"lbhd\":0,\"id0\":0,\"p0\":312,\"p1\":597,\"p2\":82,\"p3\":598,\"p4\":571,\"p5\":81,\"p6\":340,\"p7\":329,\"p8\":81,\"p9\":285},\"t\":{\"xuid1\":\"\",\"xuid2\":\"\",\"xuid3\":\"\",\"xuid4\":\"\",\"xuid5\":\"\",\"xuid6\":\"\",\"xuid7\":\"\",\"xuid8\":\"\"}}}";

        public const int MaxIds = 8;

        public readonly string[] BlockIds = new string[MaxIds];
        public readonly ConnectionIn In;


        public TogglerBlock()
        {
            In = new(0, Id);
        }


        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            for (int i = 0; i < BlockIds.Length; i++)
            {
                blockProperties.d.SetStringDictionaryValue($"xuid{i + 1}", BlockIds[i] ?? "");
            }
            In.AddToBlockProperties(blockProperties);

            return blockProperties;
        }
    }
}
