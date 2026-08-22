using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator.Blocks
{
    public class DigitizerBlock : LogicBlock
    {
        private const string referenceJson = "{\"i\":2418,\"u\":\"v-S0Qd086UyU6kD2418\",\"p\":{\"x\":-16,\"y\":0,\"z\":-464},\"r\":{\"x\":0,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"lbhd\":0,\"xt1\":1,\"id0\":0,\"id1\":0,\"p0\":591,\"p1\":82,\"p2\":592,\"p3\":571,\"p4\":312,\"p5\":81,\"p6\":285,\"p7\":81,\"p8\":287},\"t\":{}}}";


        public bool Invert;
        public readonly ConnectionIn In;
        public readonly ConnectionOut Out;

        public DigitizerBlock()
        {
            In = new(0, Id);
            Out = new(1, Id);
        }


        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            blockProperties.d.SetBoolDictionaryValue("xt1", Invert);
            In.AddToBlockProperties(blockProperties);
            Out.AddToBlockProperties(blockProperties);

            return blockProperties;
        }
    }
}
