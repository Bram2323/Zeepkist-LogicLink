using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator.Blocks
{
    public class MemoryBlock : LogicBlock
    {
        private const string referenceJson = "{\"i\":2431,\"u\":\"1betV9oJ8EaQorb2431\",\"p\":{\"x\":-16,\"y\":0,\"z\":-432},\"r\":{\"x\":0,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"lbhd\":0,\"id0\":0,\"id1\":0,\"id2\":0,\"p0\":571,\"p1\":591,\"p2\":82,\"p3\":592,\"p4\":312,\"p5\":593,\"p6\":312,\"p7\":592,\"p8\":81,\"p9\":285,\"p10\":81,\"p11\":285,\"p12\":81,\"p13\":287},\"t\":{\"oi2\":\"0\"}}}";

        public long DefaultValue;
        public readonly ConnectionIn In;
        public readonly ConnectionIn Write;
        public readonly ConnectionOut Out;

        public MemoryBlock()
        {
            In = new(0, Id);
            Write = new(1, Id);
            Out = new(2, Id);
        }


        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            blockProperties.d.SetStringDictionaryValue("oi2", DefaultValue.ToString());
            In.AddToBlockProperties(blockProperties);
            Write.AddToBlockProperties(blockProperties);

            return blockProperties;
        }
    }
}
