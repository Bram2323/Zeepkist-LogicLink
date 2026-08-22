using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator.Blocks
{
    internal class ReferenceBlock : LogicBlock
    {
        private const string referenceJson = "";


        public readonly ConnectionIn In;
        public readonly ConnectionOut Out;

        public ReferenceBlock()
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
