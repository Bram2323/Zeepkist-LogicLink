using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator.Blocks
{
    public class TransistorBlock : LogicBlock
    {
        private const string referenceJson = "{\"i\":2419,\"u\":\"D597qW5TXEmtVQa2419\",\"p\":{\"x\":0,\"y\":0,\"z\":0},\"r\":{\"x\":0,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"lbhd\":0,\"id0\":0,\"id1\":0,\"id2\":0,\"p0\":571,\"p1\":591,\"p2\":82,\"p3\":81,\"p4\":592,\"p5\":312,\"p6\":81,\"p7\":285,\"p8\":81,\"p9\":285,\"p10\":81,\"p11\":287,\"p12\":312},\"t\":{}}}";


        public readonly ConnectionIn In;
        public readonly ConnectionIn Enable;
        public readonly ConnectionOut Out;

        public TransistorBlock()
        {
            In = new(0, Id);
            Enable = new(1, Id);
            Out = new(2, Id);
        }


        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            In.AddToBlockProperties(blockProperties);
            Enable.AddToBlockProperties(blockProperties);
            Out.AddToBlockProperties(blockProperties);

            return blockProperties;
        }
    }
}
