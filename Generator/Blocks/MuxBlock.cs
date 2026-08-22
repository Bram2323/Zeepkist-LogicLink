using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator.Blocks
{
    public class MuxBlock : LogicBlock
    {
        private const string referenceJson = "{\"i\":2436,\"u\":\"UfKKkb6Bvku2f7e2436\",\"p\":{\"x\":-16,\"y\":0,\"z\":-432},\"r\":{\"x\":0,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"p0\":591,\"p1\":82,\"p2\":571,\"p3\":592,\"p4\":312,\"p5\":81,\"p6\":287,\"p7\":81,\"p8\":285,\"p9\":81,\"p10\":285,\"p11\":81,\"p12\":285,\"p13\":81,\"p14\":285,\"p15\":81,\"p16\":285,\"p17\":81,\"p18\":287,\"lbhd\":0,\"id0\":0,\"id1\":0,\"id2\":0,\"id3\":0,\"id4\":0,\"id5\":0},\"t\":{}}}";


        public readonly ConnectionIn In1;
        public readonly ConnectionIn In2;
        public readonly ConnectionIn In3;
        public readonly ConnectionIn In4;
        public readonly ConnectionIn Selector;
        public readonly ConnectionOut Out;

        public ConnectionIn[] Inputs => [In1, In2, In3, In4];

        public MuxBlock()
        {
            In1 = new(0, Id);
            In2 = new(1, Id);
            In3 = new(2, Id);
            In4 = new(3, Id);
            Selector = new(4, Id);
            Out = new(5, Id);
        }


        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            In1.AddToBlockProperties(blockProperties);
            In2.AddToBlockProperties(blockProperties);
            In3.AddToBlockProperties(blockProperties);
            In4.AddToBlockProperties(blockProperties);
            Selector.AddToBlockProperties(blockProperties);
            Out.AddToBlockProperties(blockProperties);

            return blockProperties;
        }
    }
}
