//#define UseHinter

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Config;
using Orvid.Compiler.TrueType.Utils;
using Orvid.Compiler.TrueType.HintingVM;
using Orvid.TrueType;

namespace Orvid.Compiler.TrueType
{
	public sealed class SimpleGlyph : Glyph
	{
		public int NumberOfContours;
		public int[] EndPointsOfContours;
		public byte[] Instructions;
		public int[] Flags;
		public Vec2[] Points;
		public bool[] OnCurve;

		public SimpleGlyph(Stream strm, int contourCount, TrueTypeFont parent, uint glyphIndex)
			: base(parent, strm, glyphIndex)
		{
			this.NumberOfContours = contourCount;
			this.Read(strm);
		}

		private void Read(Stream strm)
		{
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(strm);
			EndPointsOfContours = new int[NumberOfContours];
			for (uint i = 0; i < NumberOfContours; i++)
			{
				EndPointsOfContours[i] = rdr.ReadUInt16();
			}

			Instructions = new byte[rdr.ReadUInt16()];
			for (uint i = 0; i < Instructions.Length; i++)
			{
				Instructions[i] = rdr.ReadByte();
			}

			int numberOfPoints = EndPointsOfContours[EndPointsOfContours.Length - 1] + 1;
			Flags = new int[numberOfPoints];
			Points = new Vec2[numberOfPoints];
			OnCurve = new bool[numberOfPoints];

			int repCount = 0;
			int repFlag = 0;
			for (uint i = 0; i < numberOfPoints; i++)
			{
				if (repCount > 0)
				{
					Flags[i] = repFlag;
					repCount--;
				}
				else
				{
					Flags[i] = rdr.ReadByte();
					if ((Flags[i] & (1 << 3)) != 0)
					{
						repCount = rdr.ReadByte();
						repFlag = Flags[i];
					}
				}
				OnCurve[i] = ((Flags[i] & 1) != 0);
			}

			int last = 0;
			for (uint i = 0; i < numberOfPoints; i++)
			{
				if ((Flags[i] & (1 << 1)) != 0)
				{
					if ((Flags[i] & (1 << 4)) != 0)
					{
						last = Points[i].X = last + rdr.ReadByte();
					}
					else
					{
						last = Points[i].X = last - rdr.ReadByte();
					}
				}
				else
				{
					if ((Flags[i] & (1 << 4)) != 0)
					{
						Points[i].X = last;
					}
					else
					{
						last = Points[i].X = last + rdr.ReadInt16();
					}
				}
			}

			last = 0;
			for (uint i = 0; i < numberOfPoints; i++)
			{
				if ((Flags[i] & (1 << 2)) != 0)
				{
					if ((Flags[i] & (1 << 5)) != 0)
					{
						last = Points[i].Y = last + rdr.ReadByte();
					}
					else
					{
						last = Points[i].Y = last - rdr.ReadByte();
					}
				}
				else
				{
					if ((Flags[i] & (1 << 5)) != 0)
					{
						Points[i].Y = last;
					}
					else
					{
						last = Points[i].Y = last + rdr.ReadInt16();
					}
				}
			}
		}
	}
}
