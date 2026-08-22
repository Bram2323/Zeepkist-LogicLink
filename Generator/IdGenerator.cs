using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator
{
    public static class IdGenerator
    {
        private const string PreFix = "LogicLink_";

        public static string NewId()
        {
            string id = PreFix;
            id += PlayerManager.Instance.GenerateUniqueIDforBlocks("");
            return id;
        }
    }
}
