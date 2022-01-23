using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomFileReader
{
	internal abstract class EndianAmbiguousStreamReader : BinaryReader
	{
		abstract internal byte GetByte();
		abstract internal ushort GetUShort();
		abstract internal int GetInt();
		abstract internal double GetDouble();
		abstract internal float GetFloat();
		abstract internal string GetString(int length);
		public EndianAmbiguousStreamReader(Stream Input) : base(Input) { }
	}
	internal class BigEndianStreamReader : EndianAmbiguousStreamReader
	{
		internal override byte GetByte() { return ReadByte(); }
		internal override ushort GetUShort()
		{
			byte b0 = ReadByte();
			byte b1 = ReadByte();
			return (ushort)((b0 << 8) | b1);
		}
		internal override double GetDouble()
		{
			byte b0 = ReadByte();
			byte b1 = ReadByte();
			byte b2 = ReadByte();
			byte b3 = ReadByte();
			byte b4 = ReadByte();
			byte b5 = ReadByte();
			byte b6 = ReadByte();
			byte b7 = ReadByte();
			return (double)((b0 << 56) | (b1 << 48) | (b2 << 40) | (b3 << 32) | (b4 << 24) | (b5 << 16) | (b6 << 8) | b7);
		}
		internal override float GetFloat()
		{
			byte b0 = ReadByte();
			byte b1 = ReadByte();
			byte b2 = ReadByte();
			byte b3 = ReadByte();
			return (float)((b0 << 24) | (b1 << 16) | (b2 << 8) | b3);
		}
		internal override int GetInt()
		{
			byte b0 = ReadByte();
			byte b1 = ReadByte();
			byte b2 = ReadByte();
			byte b3 = ReadByte();
			return (int)((b0 << 24) | (b1 << 16) | (b2 << 8) | b3);
		}
		internal override string GetString(int length)
		{
			byte[] bytes = new byte[length];
			Read(bytes, 0, length);
			//Array.Reverse(bytes); //lol no big endian would only affect UTF-16/Unicode, but this is all 
			return System.Text.Encoding.UTF8.GetString(bytes);
		}


		public BigEndianStreamReader(Stream Input) : base(Input) { }
	}
	internal class LittleEndianStreamReader : EndianAmbiguousStreamReader
	{
		internal override byte GetByte() { return ReadByte(); }
		internal override ushort GetUShort() { return ReadUInt16(); }
		internal override int GetInt() { return ReadInt32(); }
		internal override float GetFloat() { return ReadSingle(); }
		internal override double GetDouble() { return ReadDouble(); }
		internal override string GetString(int length)
		{
			byte[] bytes = new byte[length];
			Read(bytes, 0, length);
			return System.Text.Encoding.UTF8.GetString(bytes);
		}

		public LittleEndianStreamReader(Stream Input) : base(Input) { }
	}
}
