using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType
{
	/// <summary>
	/// Represents a single glyph.
	/// </summary>
	public abstract class Glyph
	{
		private readonly TrueTypeFont local_ParentFont;
		/// <summary>
		/// The parent for this font.
		/// </summary>
		public TrueTypeFont ParentFont { get { return local_ParentFont; } }
		public int MinX;
		public int MinY;
		public int MaxX;
		public int MaxY;

		public uint GlyphIndex;


		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Glyph"/> class.
		/// </summary>
		/// <param name="parent">The parent for this glyph.</param>
		/// <param name="strm">The stream to read the glyph from.</param>
		/// <param name="glyphIndex">The index of this glyph in the stream.</param>
		public Glyph(TrueTypeFont parent, Stream strm, uint glyphIndex)
		{
			this.local_ParentFont = parent;
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(strm);
			MinX = rdr.ReadInt16();
			MinY = rdr.ReadInt16();
			MaxX = rdr.ReadInt16();
			MaxY = rdr.ReadInt16();
			this.GlyphIndex = glyphIndex;
		}

		///// <summary>
		///// Gets the rendered version of this
		///// glyph.
		///// </summary>
		///// <returns>A rendered version of this glyph.</returns>
		//public abstract Image GetRendering(double size);
	}
}
