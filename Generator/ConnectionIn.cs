using I2.Loc.SimpleJSON;
using Newtonsoft.Json;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator
{
    public class ConnectionIn
    {
        private ConnectionStruct? Connection;
        public readonly int Pin;
        public readonly string BlockId;


        public ConnectionIn(int pin, string blockId)
        {
            Pin = pin;
            BlockId = blockId;
        }


        public ConnectionIn(int pin)
        {
            Pin = pin;
        }

        public void SetConnection(string blockId, int pinId)
        {
            Connection = new(blockId, pinId, false);
        }

        public void RemoveConnection()
        {
            Connection = null;
        }

        public bool ConnectionExists()
        {
            return Connection != null;
        }


        public void AddToBlockProperties(BlockPropertyJSON blockProperties)
        {
            if (Connection == null)
            {
                blockProperties.d.SetIntDictionaryValue($"id{Pin}", 0);
                return;
            }

            string json = JsonConvert.SerializeObject(Connection);
            blockProperties.d.SetStringDictionaryValue($"id{Pin}-0", json);
            blockProperties.d.SetIntDictionaryValue($"id{Pin}", 1);
        }
    }
}
