using System;
using System.Collections.Generic;

namespace NBT.Tag
{
    public enum MatertialsType
    {
        CLASSIC,
        ALPHA
    }

    public struct SCHEMATICTag
    {
        short                   x;
        short                   z;
        short                   y;

        public short            X
        {
            get { return this.x; }
        }
        public short            Z
        {
            get { return this.z; }
        }
        public short            Y
        {
            get { return this.y; }
        }

        MatertialsType          material;

        public MatertialsType   MaterialsType
        {
            get { return this.material; }
            set { this.material = value; }
        }

        byte[]                  blocks;
        byte[]                  data;

        public byte[]           Blocks
        {
            get { return this.blocks; }
            set { this.blocks = value; }
        }
        public byte[]           Data
        {
            get { return this.data; }
            set { this.data = value; }
        }

        List<NBTTag>            entities;
        List<NBTTag>            tileents;

        public List<NBTTag>     Entities
        {
            get { return this.entities; }
            set { this.entities = value; }
        }
        public List<NBTTag>     TileEntities
        {
            get { return this.tileents; }
            set { this.tileents = value; }
        }
    }
}
