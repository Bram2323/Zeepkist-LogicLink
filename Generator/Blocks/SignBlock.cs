using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator.Blocks
{
    internal class SignBlock : Block
    {
        private const string referenceJson = "{\"i\":2381,\"u\":\"sign\",\"p\":{\"x\":0,\"y\":0,\"z\":0},\"r\":{\"x\":90,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"p0\":434,\"p1\":316,\"p2\":434,\"p3\":316,\"p4\":434,\"p5\":316,\"a0\":0,\"a1\":0,\"a2\":1},\"t\":{\"sg\":\"text\"}}}";

        public string Text = "";


        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            blockProperties.d.SetStringDictionaryValue("sg", Text);

            return blockProperties;
        }
    }
}
