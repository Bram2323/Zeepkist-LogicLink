using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator
{
    public abstract class LogicBlock : Block
    {
        public bool HideInGame = true;

        public override BlockPropertyJSON GetBlockProperties()
        {
            BlockPropertyJSON properties = base.GetBlockProperties();
            properties.d.SetBoolDictionaryValue("lbhd", HideInGame);
            return properties;
        }
    }
}
