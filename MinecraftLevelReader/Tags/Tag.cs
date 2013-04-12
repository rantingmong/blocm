using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MinecraftLevelReader.Tags
{
	public abstract class Tag
	{
		public static readonly byte TAG_End			= 0;
		public static readonly byte TAG_Byte		= 1;
		public static readonly byte TAG_Short		= 2;
		public static readonly byte TAG_Int			= 3;
		public static readonly byte TAG_Long		= 4;
		public static readonly byte TAG_Float		= 5;
		public static readonly byte TAG_Double		= 6;
		public static readonly byte TAG_Byte_Array	= 7;
		public static readonly byte TAG_String		= 8;
		public static readonly byte TAG_List		= 9;
		public static readonly byte TAG_Compound	= 10;
		public static readonly byte TAG_Int_Array	= 11;

		public abstract byte		Id		{ get; set; }
		public abstract string		Name	{ get; set; }

		public abstract object		Data	{ get; set; }

		protected Tag(string name)
		{
			if (name == null)
				name = "";
			else
				Name = name;
		}

		public abstract void Write(Stream stream);
		public abstract void Read(Stream stream);

	}
}
