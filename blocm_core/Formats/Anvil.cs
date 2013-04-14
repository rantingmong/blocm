using System.Collections.Generic;

namespace NBT.Formats
{
	public struct Anvil
	{
		public List<NbtTag>			Entities			{ get; set; }
		public List<NbtTag>			TileEntities		{ get; set; } 

		public byte[]				Biomes				{ get; set; }

		public long					LastUpdate			{ get; set; }
		
		public int					XPos				{ get; set; }
		public int					ZPos				{ get; set; }

		public byte					TerrainPopulated	{ get; set; }

		public int[]				HeightMap			{ get; set; }

		public List<AnvilSection>	Sections			{ get; set; }

		public Anvil(NbtFile input)
			: this()
		{
			Entities			= input["Level"].Payload["Entities"].Payload;
			TileEntities		= input["Level"].Payload["TileEntities"].Payload;

			Biomes				= input["Level"].Payload["Biomes"].Payload;
								  
			LastUpdate			= input["Level"].Payload["LastUpdate"].Payload;
								  
			XPos				= input["Level"].Payload["xPos"].Payload;
			ZPos				= input["Level"].Payload["zPos"].Payload;
								  
			TerrainPopulated	= input["Level"].Payload["TerrainPopulated"].Payload;
								  
			HeightMap			= input["Level"].Payload["HeightMap"].Payload;

			Sections			= new List<AnvilSection>();

			foreach (NbtTag section in input["Level"].Payload["Sections"].Payload)
			{
				AnvilSection anvilSection = new AnvilSection
					{
						Data		= new byte[4096],
						SkyLight	= new byte[4096],
						BlockLight	= new byte[4096],
						Y			= section.Payload["Y"].Payload,
						Blocks		= section.Payload["Blocks"].Payload,
					};

				byte[] aData = section.Payload["Data"].Payload;
				byte[] sData = section.Payload["SkyLight"].Payload;
				byte[] bData = section.Payload["BlockLight"].Payload;

				int index = 0;

				for (int i = 0; i < 2048; i++)
				{
					byte nibbleHi = (byte) (aData[i] & 0X0F);
					byte nibbleLo = (byte) (aData[i] >> 8);

					anvilSection.Data[index++] = nibbleHi;
					anvilSection.Data[index++] = nibbleLo;
				}

				index = 0;

				for (int i = 0; i < 2048; i++)
				{
					byte nibbleHi = (byte)(sData[i] & 0X0F);
					byte nibbleLo = (byte)(sData[i] >> 8);

					anvilSection.SkyLight[index++] = nibbleHi;
					anvilSection.SkyLight[index++] = nibbleLo;
				}

				index = 0;

				for (int i = 0; i < 2048; i++)
				{
					byte nibbleHi = (byte)(bData[i] & 0X0F);
					byte nibbleLo = (byte)(bData[i] >> 8);

					anvilSection.BlockLight[index++] = nibbleHi;
					anvilSection.BlockLight[index++] = nibbleLo;
				}

				Sections.Add(anvilSection);
			}
		}
	}

	public struct AnvilSection
	{
		public byte[]	Data		{ get; set; }
		public byte[]	SkyLight	{ get; set; }
		public byte[]	BlockLight	{ get; set; }

		public byte		Y			{ get; set; }

		public byte[]	Blocks		{ get; set; }
	}
}
