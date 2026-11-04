using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator.Blocks
{
    internal class CounterBlock : LogicBlock
    {
        private const string referenceJson = "{\"i\":2434,\"u\":\"eqHuWfBQKkCI4Xc2434\",\"p\":{\"x\":0,\"y\":0,\"z\":0},\"r\":{\"x\":0,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"p0\":81,\"p1\":340,\"p2\":335,\"p3\":591,\"p4\":82,\"p5\":571,\"p6\":592,\"p7\":312,\"p8\":81,\"p9\":285,\"p10\":81,\"p11\":285,\"p12\":81,\"p13\":285,\"p14\":81,\"p15\":287,\"lbhd\":0,\"id0\":0,\"id1\":0,\"id2\":0,\"id3\":0},\"t\":{\"ld1\":\"0\",\"ld2\":\"0\"}}}";


        public readonly ConnectionIn CountUp;
        public readonly ConnectionIn CountDown;
        public readonly ConnectionIn Reset;
        public readonly ConnectionOut Out;

        public long MaxThreshold = 0;
        public long MinThreshold = 0;

        public CounterBlock()
        {
            CountUp = new(0, Id);
            CountDown = new(1, Id);
            Reset = new(2, Id);
            Out = new(3, Id);
        }


        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            blockProperties.d.SetStringDictionaryValue("ld1", MaxThreshold.ToString());
            blockProperties.d.SetStringDictionaryValue("ld2", MinThreshold.ToString());

            CountUp.AddToBlockProperties(blockProperties);
            CountDown.AddToBlockProperties(blockProperties);
            Reset.AddToBlockProperties(blockProperties);
            Out.AddToBlockProperties(blockProperties);

            return blockProperties;
        }
    }
}
