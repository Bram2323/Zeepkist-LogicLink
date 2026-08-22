using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Generator
{
    public class ConnectionOut
    {
        private List<ConnectionStruct> Connections = [];
        public readonly int Pin;
        public readonly string BlockId;


        public ConnectionOut(int pin, string blockId)
        {
            Pin = pin;
            BlockId = blockId;
        }

        public void AddConnection(string blockId, int pinId)
        {
            Connections.Add(new(blockId, pinId, true));
        }

        public void RemoveConnection(string blockId, int pinId)
        {
            int index = Connections.FindIndex((connection) => connection.targetUID == blockId && connection.connectorID == pinId);
            if (index >= 0 && index < Connections.Count) Connections.RemoveAt(index);
        }

        public bool ConnectionExists(string blockId, int pinId)
        {
            int index = Connections.FindIndex((connection) => connection.targetUID == blockId && connection.connectorID == pinId);
            return index >= 0 && index < Connections.Count;
        }


        public void ConnectTo(ConnectionIn connection)
        {
            AddConnection(connection.BlockId, connection.Pin);
            connection.SetConnection(BlockId, Pin);
        }


        public void AddToBlockProperties(BlockPropertyJSON blockProperties)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                ConnectionStruct connection = Connections[i];
                string json = JsonConvert.SerializeObject(connection);
                blockProperties.d.SetStringDictionaryValue($"id{Pin}-{i}", json);
            }
            blockProperties.d.SetIntDictionaryValue($"id{Pin}", Connections.Count);
        }
    }
}
