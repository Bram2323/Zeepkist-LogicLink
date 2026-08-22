using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator.Blocks
{
    public class PulseGeneratorBlock : LogicBlock
    {
        private const string referenceJson = "{\"i\":2439,\"u\":\"8V13nfoC9E61AM42439\",\"p\":{\"x\":-16,\"y\":0,\"z\":-448},\"r\":{\"x\":0,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"lbhd\":0,\"xpl\":2,\"id0\":0,\"id1\":0,\"p0\":303,\"p1\":287,\"p2\":591,\"p3\":82,\"p4\":571,\"p5\":592,\"p6\":81,\"p7\":81,\"p8\":285,\"p9\":81,\"p10\":287},\"t\":{}}}";

        public TriggerTypes TriggerType;
        public readonly ConnectionIn In;
        public readonly ConnectionOut Out;

        public PulseGeneratorBlock()
        {
            In = new(0, Id);
            Out = new(1, Id);
        }


        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            blockProperties.d.SetIntDictionaryValue("xpl", (int)TriggerType);
            In.AddToBlockProperties(blockProperties);
            Out.AddToBlockProperties(blockProperties);

            return blockProperties;
        }


        public enum TriggerTypes
        {
            RisingEdge = 0,
            FallingEdge = 1,
            AnyRisingEdge = 2,
            AnyFallingEdge = 3,
            AnyIncrement = 4,
            AnyDecrement = 5,
        }
    }
}
