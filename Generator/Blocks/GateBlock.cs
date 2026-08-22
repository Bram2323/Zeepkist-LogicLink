using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogicLink.Generator.Blocks
{
    public class GateBlock : LogicBlock
    {
        private const string referenceJson = "{\"i\":2420,\"u\":\"Pi0Reg68OEe5IMP2420\",\"p\":{\"x\":-16,\"y\":0,\"z\":-464},\"r\":{\"x\":0,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"p0\":591,\"p1\":82,\"p2\":571,\"p3\":592,\"p4\":312,\"p5\":81,\"p6\":81,\"p7\":285,\"p8\":81,\"p9\":285,\"p10\":81,\"p11\":285,\"p12\":81,\"p13\":287,\"lbhd\":0,\"xmo\":1,\"id0\":0,\"id1\":0,\"id2\":0,\"id3\":0},\"t\":{}}}";

        public Opperators Opperator = Opperators.And;
        public long Num2Override;
        public readonly ConnectionIn Num1;
        public readonly ConnectionIn Num2;
        public readonly ConnectionIn OpperatorIn;
        public readonly ConnectionOut Out;


        public GateBlock()
        {
            Num1 = new(0, Id);
            Num2 = new(1, Id);
            OpperatorIn = new(2, Id);
            Out = new(3, Id);
        }


        public override BlockPropertyJSON GetBlockSpecificProperties()
        {
            BlockPropertyJSON blockProperties = JsonConvert.DeserializeObject<BlockPropertyJSON>(referenceJson);

            blockProperties.d.SetIntDictionaryValue("xmo", (int)Opperator);
            blockProperties.d.SetStringDictionaryValue("oi2", Num2Override.ToString());
            Num1.AddToBlockProperties(blockProperties);
            Num2.AddToBlockProperties(blockProperties);
            OpperatorIn.AddToBlockProperties(blockProperties);
            Out.AddToBlockProperties(blockProperties);

            return blockProperties;
        }


        public enum Opperators
        {
            And = 0,
            Or = 1,
            Nand = 2,
            Nor = 3,
            Xor = 4,
            Xnor = 5,
        }
    }
}
