using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator.Blocks
{
    public class DelayBlock : LogicBlock
    {
        private const string referenceJson = "{\"i\":2428,\"u\":\"bZjcftPjMk-AjLf2428\",\"p\":{\"x\":-16,\"y\":0,\"z\":-432},\"r\":{\"x\":0,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"lbhd\":0,\"id0\":0,\"id1\":0,\"p0\":591,\"p1\":82,\"p2\":592,\"p3\":571,\"p4\":312,\"p5\":310,\"p6\":309,\"p7\":81,\"p8\":285,\"p9\":81,\"p10\":287},\"t\":{\"ldl\":\"5\"}}}";

        public int Delay;
        public readonly ConnectionIn In;
        public readonly ConnectionOut Out;

        public DelayBlock()
        {
            In = new(0, Id);
            Out = new(1, Id);
        }


        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            blockProperties.d.SetStringDictionaryValue("ldl", Delay.ToString());
            In.AddToBlockProperties(blockProperties);
            Out.AddToBlockProperties(blockProperties);

            return blockProperties;
        }
    }
}
