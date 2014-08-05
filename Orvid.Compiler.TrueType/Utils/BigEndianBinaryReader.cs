using System;
using System.IO;
using System.Text;

namespace Orvid.Compiler.TrueType.Utils
{
	/// <summary>
	/// Reads data in big endian format.
	/// </summary>
	public sealed class BigEndianBinaryReader
	{
		public double ReadFixed()
		{
			return ((double)this.ReadUInt16()) + (((double)this.ReadUInt16()) / 16384);
		}

		public string ReadASCIIChars(int count)
		{
			return ASCIIEncoding.ASCII.GetString(ReadBytes(count));
		}

		public double ReadF2Dot14()
		{
			int major = this.ReadByte();
			int minor = this.ReadByte();
			double fract = minor + ((major & 0x3F) << 8);
			double mant = major >> 6;
			if (mant >= 2)
				mant -= 4;
			return mant + (fract / 16384);
		}

		byte[] buffer = new byte[16];

		public BigEndianBinaryReader(Stream stream)
		{
			this.stream = stream;
		}

		Stream stream;
		public Stream BaseStream
		{
			get { return stream; }
		}

		public byte ReadByte()
		{
			stream.Read(buffer, 0, 1);
			return buffer[0];
		}

		public sbyte ReadSByte()
		{
			stream.Read(buffer, 0, 1);
			return unchecked((sbyte)buffer[0]);
		}

		public bool ReadBoolean()
		{
			stream.Read(buffer, 0, 1);
			return BitConverter.ToBoolean(buffer, 0);
		}

		public short ReadInt16()
		{
			stream.Read(buffer, 0, 2);
			Array.Reverse(buffer, 0, 2);
			return BitConverter.ToInt16(buffer, 0);
		}

		public int ReadInt32()
		{
			stream.Read(buffer, 0, 4);
			Array.Reverse(buffer, 0, 4);
			return BitConverter.ToInt32(buffer, 0);
		}

		public long ReadInt64()
		{
			stream.Read(buffer, 0, 8);
			Array.Reverse(buffer, 0, 8);
			return BitConverter.ToInt64(buffer, 0);
		}

		public ushort ReadUInt16()
		{
			stream.Read(buffer, 0, 2);
			Array.Reverse(buffer, 0, 2);
			return BitConverter.ToUInt16(buffer, 0);
		}

		public uint ReadUInt32()
		{
			stream.Read(buffer, 0, 4);
			Array.Reverse(buffer, 0, 4);
			return BitConverter.ToUInt32(buffer, 0);
		}

		public ulong ReadUInt64()
		{
			stream.Read(buffer, 0, 8);
			Array.Reverse(buffer, 0, 8);
			return BitConverter.ToUInt64(buffer, 0);
		}

		public int Read(byte[] buffer, int offset, int length)
		{
			return stream.Read(buffer, offset, length);
		}

		public byte[] ReadBytes(int count)
		{
			byte[] buf = new byte[count];
			int read = stream.Read(buf, 0, count);
			if (read == 0)
				throw new Exception("Pre-mature end to the stream!");
			return buf;
		}

	}
}
