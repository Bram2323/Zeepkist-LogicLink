using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator.Blocks
{
    internal class MusicBlock : Block
    {
        private const string referenceJson = "{\"i\":2279,\"u\":\"iu66kuPR0EKM_oi2279\",\"p\":{\"x\":-16,\"y\":0,\"z\":-464},\"r\":{\"x\":0,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"n6\":4,\"n8\":1}}}";

        public Shapes Shape = Shapes.Block;
        public Instruments Instrument = Instruments.Piano;
        public int Note = 0;

        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            blockProperties.d.SetBoolDictionaryValue($"a{(int)Shape}", true);
            blockProperties.d.SetIntDictionaryValue($"n6", (int)Instrument);
            blockProperties.d.SetIntDictionaryValue($"n8", Note);

            return blockProperties;
        }


        public enum Shapes
        {
            Block = 0,
            Cylinder = 1,
            Sphere = 2,
            Wedge = 3,
        }

        public enum Instruments
        {
            Piano = 0,
            Trumpet = 1,
            Flute = 3,
            Kazoo = 4,
            Blarhgl = 5,
        }
    }
}
