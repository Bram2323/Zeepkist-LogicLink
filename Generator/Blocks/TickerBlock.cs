using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator.Blocks
{
    public class TickerBlock : LogicBlock
    {
        private const string referenceJson = "{\"i\":2429,\"u\":\"m5qZ8e2_wkmxMec2429\",\"p\":{\"x\":-16,\"y\":0,\"z\":-464},\"r\":{\"x\":0,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"p0\":606,\"p1\":82,\"p2\":607,\"p3\":571,\"p4\":81,\"p5\":312,\"p6\":287,\"p7\":340,\"p8\":335,\"p9\":81,\"p10\":287,\"lbhd\":0,\"id0\":0},\"t\":{}}}";


        public readonly ConnectionOut Out;

        public TickerBlock()
        {
            Out = new(0, Id);
        }


        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            Out.AddToBlockProperties(blockProperties);

            return blockProperties;
        }
    }
}
