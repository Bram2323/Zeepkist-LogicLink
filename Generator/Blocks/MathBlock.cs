using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogicLink.Generator.Blocks
{
    public class MathBlock : LogicBlock
    {
        private const string referenceJson = "{\"i\":2416,\"u\":\"YR2liruB6ku-vWy2416\",\"p\":{\"x\":-32,\"y\":0,\"z\":-432},\"r\":{\"x\":0,\"y\":0,\"z\":0},\"s\":{\"x\":1,\"y\":1,\"z\":1},\"d\":{\"n\":{\"p0\":591,\"p1\":82,\"p2\":571,\"p3\":592,\"p4\":312,\"p5\":81,\"p6\":329,\"p7\":335,\"p8\":81,\"p9\":285,\"p10\":81,\"p11\":285,\"p12\":81,\"p13\":285,\"p14\":81,\"p15\":287,\"lbhd\":0,\"xmo\":2,\"id0\":0,\"id1\":0,\"id2\":0,\"id3\":0},\"t\":{\"oi2\":\"5\"}}}";

        public Opperators Opperator = Opperators.Add;
        public long Num2Override;
        public readonly ConnectionIn Num1;
        public readonly ConnectionIn Num2;
        public readonly ConnectionIn OpperatorIn;
        public readonly ConnectionOut Out;


        public MathBlock()
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
            Add = 0,
            Subtract = 1,
            Multiply = 2,
            Divide = 3,
            Modulo = 4,
            LessThan = 5,
            GreaterThan = 6,
            LessThanOrEqual = 7,
            GreaterThanOrEqual = 8,
            Equal = 9,
            NotEqual = 10,
            Min = 11,
            Max = 12,
            Pow = 13,
            Log = 14,
            Abs = 15,
            Sign = 16,
            Repeat = 17,
            PingPong = 18,
            Sin = 19,
            Cos = 20,
            Tan = 21,
            Asin = 22,
            Acos = 23,
            Atan = 24,
            Atan2 = 25,
            Sqrt = 26,
            DecimalDivide = 27,
        }
    }
}
