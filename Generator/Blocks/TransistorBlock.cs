using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator.Blocks
{
    public class TransistorBlock : LogicBlock
    {
        private const string referenceJson = "";


        public readonly ConnectionIn In;
        public readonly ConnectionIn Enable;
        public readonly ConnectionOut Out;

        public TransistorBlock()
        {
            In = new(0, Id);
            Enable = new(1, Id);
            Out = new(2, Id);
        }


        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            In.AddToBlockProperties(blockProperties);
            Enable.AddToBlockProperties(blockProperties);
            Out.AddToBlockProperties(blockProperties);

            return blockProperties;
        }
    }
}
