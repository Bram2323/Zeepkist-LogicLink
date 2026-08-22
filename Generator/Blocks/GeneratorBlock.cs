using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogicLink.Generator.Blocks
{
    public class GeneratorBlock : LogicBlock
    {
        private const string referenceJson = "{\"i\":2414,\"u\":\"SrVpboL6fEWDkcg2414\",\"p\":{\"x\":-16,\"y\":0,\"z\":-448},\"r\":{\"x\":0,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"p0\":606,\"p1\":82,\"p2\":607,\"p3\":571,\"p4\":335,\"p5\":81,\"p6\":335,\"p7\":287,\"p8\":82,\"p9\":81,\"p10\":287,\"lbhd\":0,\"id0\":2},\"t\":{\"lbp\":\"69\"}}}";

        public long Value;
        public readonly ConnectionOut Out;


        public GeneratorBlock()
        {
            Out = new(0, Id);
        }


        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            blockProperties.d.SetStringDictionaryValue("lbp", Value.ToString());
            Out.AddToBlockProperties(blockProperties);

            return blockProperties;
        }
    }
}
