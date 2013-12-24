using System.Collections.Generic;

namespace NBT.Formats
{
    public struct Anvil
    {
        public Anvil(NbtFile input)
            : this()
        {
            var payload = (Dictionary<string, NbtTag>)input["Level"].Payload;

            Entities = (List<NbtTag>)payload["Entities"].Payload;
            TileEntities = (List<NbtTag>)payload["TileEntities"].Payload;
            Biomes = (byte[])payload["Biomes"].Payload;
            LastUpdate = (long)payload["LastUpdate"].Payload;
            XPos = (int)payload["xPos"].Payload;
            ZPos = (int)payload["zPos"].Payload;
            TerrainPopulated = (byte)payload["TerrainPopulated"].Payload;
            HeightMap = (int[])payload["HeightMap"].Payload;
            Sections = new List<AnvilSection>();

            foreach (var section in (List<NbtTag>)payload["Sections"].Payload)
            {
                var content = (Dictionary<string, NbtTag>)section.Payload;

                var anvilSection = new AnvilSection
                    {
                        Data = new byte[4096],
                        SkyLight = new byte[4096],
                        BlockLight = new byte[4096],
                        Y = (byte)content["Y"].Payload,
                        Blocks = (byte[])content["Blocks"].Payload,
                    };

                var aData = (byte[])content["Data"].Payload;
                var sData = (byte[])content["SkyLight"].Payload;
                var bData = (byte[])content["BlockLight"].Payload;

                var index = 0;

                for (int i = 0; i < 2048; i++)
                {
                    var nibbleHi = (byte)(aData[i] & 0X0F);
                    var nibbleLo = (byte)(aData[i] >> 8);

                    anvilSection.Data[index++] = nibbleHi;
                    anvilSection.Data[index++] = nibbleLo;
                }

                index = 0;

                for (int i = 0; i < 2048; i++)
                {
                    var nibbleHi = (byte)(sData[i] & 0X0F);
                    var nibbleLo = (byte)(sData[i] >> 8);

                    anvilSection.SkyLight[index++] = nibbleHi;
                    anvilSection.SkyLight[index++] = nibbleLo;
                }

                index = 0;

                for (int i = 0; i < 2048; i++)
                {
                    var nibbleHi = (byte)(bData[i] & 0X0F);
                    var nibbleLo = (byte)(bData[i] >> 8);

                    anvilSection.BlockLight[index++] = nibbleHi;
                    anvilSection.BlockLight[index++] = nibbleLo;
                }

                Sections.Add(anvilSection);
            }
        }

        public List<NbtTag> Entities { get; set; }
        public List<NbtTag> TileEntities { get; set; }

        public byte[] Biomes { get; set; }

        public long LastUpdate { get; set; }

        public int XPos { get; set; }
        public int ZPos { get; set; }

        public byte TerrainPopulated { get; set; }

        public int[] HeightMap { get; set; }

        public List<AnvilSection> Sections { get; set; }
    }

    public struct AnvilSection
    {
        public byte[] Data { get; set; }
        public byte[] SkyLight { get; set; }
        public byte[] BlockLight { get; set; }

        public byte Y { get; set; }

        public byte[] Blocks { get; set; }
    }
}
