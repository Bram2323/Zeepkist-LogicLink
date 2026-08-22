using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogicLink.Generator
{
    public abstract class Block
    {
        public readonly string Id = IdGenerator.NewId();

        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = Vector3.one;


        public virtual BlockPropertyJSON GetBlockProperties()
        {
            BlockPropertyJSON properties = GetBlockSpecificProperties();

            properties.u = Id;
            properties.p = new(Position);
            properties.r = new(Rotation);
            properties.s = new(Scale);

            return properties;
        }

        public abstract BlockPropertyJSON GetBlockSpecificProperties();
    }
}
