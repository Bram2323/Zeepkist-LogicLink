using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator.Blocks
{
    public class PassthroughBlock : LogicBlock
    {
        private const string referenceJson = "{\"i\":2426,\"u\":\"a_UaK0P7T0ifezA0\",\"p\":{\"x\":0,\"y\":0,\"z\":-416},\"r\":{\"x\":0,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"p0\":591,\"p1\":82,\"p2\":571,\"p3\":81,\"p4\":285,\"p5\":81,\"p6\":287,\"lbhd\":0,\"id0\":1,\"id1\":0}}}";


        public readonly ConnectionIn In;
        public readonly ConnectionOut Out;

        public PassthroughBlock()
        {
            In = new(0, Id);
            Out = new(1, Id);
        }


        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            In.AddToBlockProperties(blockProperties);
            Out.AddToBlockProperties(blockProperties);

            return blockProperties;
        }
    }
}
