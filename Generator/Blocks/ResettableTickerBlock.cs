using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator.Blocks
{
    public class ResettableTickerBlock : LogicBlock
    {
        private const string referenceJson = "{\"i\":2433,\"u\":\"s5sg62DuJUG2sZP2433\",\"p\":{\"x\":0,\"y\":0,\"z\":0},\"r\":{\"x\":0,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"lbhd\":0,\"id0\":0,\"id1\":0,\"p0\":81,\"p1\":340,\"p2\":335,\"p3\":591,\"p4\":82,\"p5\":571,\"p6\":592,\"p7\":81,\"p8\":285,\"p9\":81,\"p10\":287},\"t\":{}}}";


        public readonly ConnectionIn Reset;
        public readonly ConnectionOut Out;

        public ResettableTickerBlock()
        {
            Reset = new(0, Id);
            Out = new(1, Id);
        }


        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            Reset.AddToBlockProperties(blockProperties);
            Out.AddToBlockProperties(blockProperties);

            return blockProperties;
        }
    }
}
