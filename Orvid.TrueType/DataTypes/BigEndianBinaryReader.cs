using System;
using System.IO;
using System.Text;

namespace Orvid.TrueType.Utils
{
	/// <summary>
	/// Equivalent of System.IO.BinaryReader, but using a big-endian byte order.
	/// No data is buffered in the reader; the client may seek within the stream
	/// at will.
	/// </summary>
	public sealed class BigEndianBinaryReader : IDisposable
	{
		/// <summary>
		/// Reads a 32-bit fixed decimal from the stream.
		/// 4 bytes are read.
		/// </summary>
		/// <returns>The fixed decimal read</returns>
		public double ReadFixed()
		{
			return ((double)this.ReadUInt16()) + (((double)this.ReadUInt16()) / 16384);
		}

		/// <summary>
		/// Reads a series of ASCII characters from a stream.
		/// </summary>
		/// <param name="count">The number of chars to read.</param>
		/// <returns>The read characters as a string.</returns>
		public string ReadASCIIChars(int count)
		{
			return ASCIIEncoding.ASCII.GetString(ReadBytes(count));
		}

		/// <summary>
		/// Reads a double in the F2Dot14
		/// format.
		/// </summary>
		/// <returns>The read double.</returns>
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

		/// <summary>
		/// Whether or not this reader has been disposed yet.
		/// </summary>
		bool disposed = false;
		/// <summary>
		/// Decoder to use for string conversions.
		/// </summary>
		Decoder decoder;
		/// <summary>
		/// Buffer used for temporary storage before conversion into primitives
		/// </summary>
		byte[] buffer = new byte[16];
		/// <summary>
		/// Buffer used for temporary storage when reading a single character
		/// </summary>
		char[] charBuffer = new char[1];
		/// <summary>
		/// Minimum number of bytes used to encode a character
		/// </summary>
		int minBytesPerChar;

		/// <summary>
		/// Equivalent of System.IO.BinaryReader, but using a big-endian byte order.
		/// </summary>
		/// <param name="stream">Stream to read data from</param>
		public BigEndianBinaryReader(Stream stream) : this(stream, Encoding.UTF8)
		{
		}

		/// <summary>
		/// Constructs a new binary reader reading from the given stream,
		/// using the given encoding.
		/// </summary>
		/// <param name="stream">Stream to read data from</param>
		/// <param name="encoding">Encoding to use when reading character data</param>
		public BigEndianBinaryReader(Stream stream, Encoding encoding)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (encoding == null)
			{
				throw new ArgumentNullException("encoding");
			}
			if (!stream.CanRead)
			{
				throw new ArgumentException("Stream isn't readable", "stream");
			}
			this.stream = stream;
			this.encoding = encoding;
			this.decoder = encoding.GetDecoder();
			this.minBytesPerChar = 1;

			if (encoding is UnicodeEncoding)
			{
				minBytesPerChar = 2;
			}
		}


		Encoding encoding;
		/// <summary>
		/// The encoding used to read strings
		/// </summary>
		public Encoding Encoding
		{
			get { return encoding; }
		}

		Stream stream;
		/// <summary>
		/// Gets the underlying stream of the EndianBinaryReader.
		/// </summary>
		public Stream BaseStream
		{
			get { return stream; }
		}

		/// <summary>
		/// Closes the reader, including the underlying stream..
		/// </summary>
		public void Close()
		{
			Dispose();
		}

		/// <summary>
		/// Seeks within the stream.
		/// </summary>
		/// <param name="offset">Offset to seek to.</param>
		/// <param name="origin">Origin of seek operation.</param>
		public void Seek(int offset, SeekOrigin origin)
		{
			CheckDisposed();
			stream.Seek(offset, origin);
		}

		/// <summary>
		/// Reads a single byte from the stream.
		/// </summary>
		/// <returns>The byte read</returns>
		public byte ReadByte()
		{
			ReadInternal(buffer, 1);
			return buffer[0];
		}

		/// <summary>
		/// Reads a single signed byte from the stream.
		/// </summary>
		/// <returns>The byte read</returns>
		public sbyte ReadSByte()
		{
			ReadInternal(buffer, 1);
			return unchecked((sbyte)buffer[0]);
		}

		/// <summary>
		/// Reads a boolean from the stream. 1 byte is read.
		/// </summary>
		/// <returns>The boolean read</returns>
		public bool ReadBoolean()
		{
			ReadInternal(buffer, 1);
			return BitConverter.ToBoolean(buffer, 0);
		}

		/// <summary>
		/// Reads a 16-bit signed integer from the stream, using the bit converter
		/// for this reader. 2 bytes are read.
		/// </summary>
		/// <returns>The 16-bit integer read</returns>
		public short ReadInt16()
		{
			ReadInternal(buffer, 2);
			Array.Reverse(buffer, 0, 2);
			return BitConverter.ToInt16(buffer, 0);
		}

		/// <summary>
		/// Reads a 32-bit signed integer from the stream, using the bit converter
		/// for this reader. 4 bytes are read.
		/// </summary>
		/// <returns>The 32-bit integer read</returns>
		public int ReadInt32()
		{
			ReadInternal(buffer, 4);
			Array.Reverse(buffer, 0, 4);
			return BitConverter.ToInt32(buffer, 0);
		}

		/// <summary>
		/// Reads a 64-bit signed integer from the stream, using the bit converter
		/// for this reader. 8 bytes are read.
		/// </summary>
		/// <returns>The 64-bit integer read</returns>
		public long ReadInt64()
		{
			ReadInternal(buffer, 8);
			Array.Reverse(buffer, 0, 8);
			return BitConverter.ToInt64(buffer, 0);
		}

		/// <summary>
		/// Reads a 16-bit unsigned integer from the stream, using the bit converter
		/// for this reader. 2 bytes are read.
		/// </summary>
		/// <returns>The 16-bit unsigned integer read</returns>
		public ushort ReadUInt16()
		{
			ReadInternal(buffer, 2);
			Array.Reverse(buffer, 0, 2);
			return BitConverter.ToUInt16(buffer, 0);
		}

		/// <summary>
		/// Reads a 32-bit unsigned integer from the stream, using the bit converter
		/// for this reader. 4 bytes are read.
		/// </summary>
		/// <returns>The 32-bit unsigned integer read</returns>
		public uint ReadUInt32()
		{
			ReadInternal(buffer, 4);
			Array.Reverse(buffer, 0, 4);
			return BitConverter.ToUInt32(buffer, 0);
		}

		/// <summary>
		/// Reads a 64-bit unsigned integer from the stream, using the bit converter
		/// for this reader. 8 bytes are read.
		/// </summary>
		/// <returns>The 64-bit unsigned integer read</returns>
		public ulong ReadUInt64()
		{
			ReadInternal(buffer, 8);
			Array.Reverse(buffer, 0, 8);
			return BitConverter.ToUInt64(buffer, 0);
		}

		/// <summary>
		/// Reads a single-precision floating-point value from the stream, using the bit converter
		/// for this reader. 4 bytes are read.
		/// </summary>
		/// <returns>The floating point value read</returns>
		public float ReadSingle()
		{
			ReadInternal(buffer, 4);
			Array.Reverse(buffer, 0, 4);
			return BitConverter.ToSingle(buffer, 0);
		}

		/// <summary>
		/// Reads a double-precision floating-point value from the stream, using the bit converter
		/// for this reader. 8 bytes are read.
		/// </summary>
		/// <returns>The floating point value read</returns>
		public double ReadDouble()
		{
			ReadInternal(buffer, 8);
			Array.Reverse(buffer, 0, 8);
			return BitConverter.ToDouble(buffer, 0);
		}

		/// <summary>
		/// Reads a single character from the stream, using the character encoding for
		/// this reader. If no characters have been fully read by the time the stream ends,
		/// -1 is returned.
		/// </summary>
		/// <returns>The character read, or -1 for end of stream.</returns>
		public int Read()
		{
			int charsRead = Read(charBuffer, 0, 1);
			if (charsRead == 0)
			{
				return -1;
			}
			else
			{
				return charBuffer[0];
			}
		}

		/// <summary>
		/// Reads the specified number of characters into the given buffer, starting at
		/// the given index.
		/// </summary>
		/// <param name="data">The buffer to copy data into</param>
		/// <param name="index">The first index to copy data into</param>
		/// <param name="count">The number of characters to read</param>
		/// <returns>
		/// The number of characters actually read. This will only be less than
		/// the requested number of characters if the end of the stream is reached.
		/// </returns>
		public int Read(char[] data, int index, int count)
		{
			CheckDisposed();
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (count + index > data.Length)
			{
				throw new ArgumentException
					("Not enough space in buffer for specified number of characters starting at specified index");
			}

			int read = 0;
			bool firstTime = true;

			// Use the normal buffer if we're only reading a small amount, otherwise
			// use at most 4K at a time.
			byte[] byteBuffer = buffer;

			if (byteBuffer.Length < count * minBytesPerChar)
			{
				byteBuffer = new byte[4096];
			}

			while (read < count)
			{
				int amountToRead;
				// First time through we know we haven't previously read any data
				if (firstTime)
				{
					amountToRead = count * minBytesPerChar;
					firstTime = false;
				}
				// After that we can only assume we need to fully read "chars left -1" characters
				// and a single byte of the character we may be in the middle of
				else
				{
					amountToRead = ((count - read - 1) * minBytesPerChar) + 1;
				}
				if (amountToRead > byteBuffer.Length)
				{
					amountToRead = byteBuffer.Length;
				}
				int bytesRead = TryReadInternal(byteBuffer, amountToRead);
				if (bytesRead == 0)
				{
					return read;
				}
				int decoded = decoder.GetChars(byteBuffer, 0, bytesRead, data, index);
				read += decoded;
				index += decoded;
			}
			return read;
		}

		/// <summary>
		/// Reads the specified number of bytes into the given buffer, starting at
		/// the given index.
		/// </summary>
		/// <param name="buffer">The buffer to copy data into</param>
		/// <param name="index">The first index to copy data into</param>
		/// <param name="count">The number of bytes to read</param>
		/// <returns>
		/// The number of bytes actually read. This will only be less than
		/// the requested number of bytes if the end of the stream is reached.
		/// </returns>
		public int Read(byte[] buffer, int index, int count)
		{
			CheckDisposed();
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (count + index > buffer.Length)
			{
				throw new ArgumentException("Not enough space in buffer for specified number of bytes starting at specified index");
			}
			int read = 0;
			while (count > 0)
			{
				int block = stream.Read(buffer, index, count);
				if (block == 0)
				{
					return read;
				}
				index += block;
				read += block;
				count -= block;
			}
			return read;
		}

		/// <summary>
		/// Reads the specified number of bytes, returning them in a new byte array.
		/// If not enough bytes are available before the end of the stream, this
		/// method will return what is available.
		/// </summary>
		/// <param name="count">The number of bytes to read</param>
		/// <returns>The bytes read</returns>
		public byte[] ReadBytes(int count)
		{
			CheckDisposed();
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			byte[] ret = new byte[count];
			int index = 0;
			while (index < count)
			{
				int read = stream.Read(ret, index, count - index);
				// Stream has finished half way through. That's fine, return what we've got.
				if (read == 0)
				{
					byte[] copy = new byte[index];
					Buffer.BlockCopy(ret, 0, copy, 0, index);
					return copy;
				}
				index += read;
			}
			return ret;
		}

		/// <summary>
		/// Reads the specified number of bytes, returning them in a new byte array.
		/// If not enough bytes are available before the end of the stream, this
		/// method will throw an IOException.
		/// </summary>
		/// <param name="count">The number of bytes to read</param>
		/// <returns>The bytes read</returns>
		public byte[] ReadBytesOrThrow(int count)
		{
			byte[] ret = new byte[count];
			ReadInternal(ret, count);
			return ret;
		}


		/// <summary>
		/// Checks whether or not the reader has been disposed, throwing an exception if so.
		/// </summary>
		private void CheckDisposed()
		{
			if (disposed)
			{
				throw new ObjectDisposedException("EndianBinaryReader");
			}
		}

		/// <summary>
		/// Reads the given number of bytes from the stream, throwing an exception
		/// if they can't all be read.
		/// </summary>
		/// <param name="data">Buffer to read into</param>
		/// <param name="size">Number of bytes to read</param>
		private void ReadInternal(byte[] data, int size)
		{
			CheckDisposed();
			int index = 0;
			while (index < size)
			{
				int read = stream.Read(data, index, size - index);
				if (read == 0)
				{
					throw new EndOfStreamException
						(String.Format("End of stream reached with {0} byte{1} left to read.", size - index,
						size - index == 1 ? "s" : ""));
				}
				index += read;
			}
		}

		/// <summary>
		/// Reads the given number of bytes from the stream if possible, returning
		/// the number of bytes actually read, which may be less than requested if
		/// (and only if) the end of the stream is reached.
		/// </summary>
		/// <param name="data">Buffer to read into</param>
		/// <param name="size">Number of bytes to read</param>
		/// <returns>Number of bytes actually read</returns>
		private int TryReadInternal(byte[] data, int size)
		{
			CheckDisposed();
			int index = 0;
			while (index < size)
			{
				int read = stream.Read(data, index, size - index);
				if (read == 0)
				{
					return index;
				}
				index += read;
			}
			return index;
		}

		/// <summary>
		/// Disposes of the underlying stream.
		/// </summary>
		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				((IDisposable)stream).Dispose();
			}
		}
	}
}
