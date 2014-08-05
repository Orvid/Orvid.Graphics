using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.Tables
{
	internal class MaximumProfileTable : ITable
	{
		public string TableTag
		{
			get { return "maxp"; }
		}

		public void LoadTable(Stream strm, uint length, TrueTypeFont fnt)
		{
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(strm);
			fnt.MaxP_Version = rdr.ReadFixed();
			if (fnt.MaxP_Version == 0.5)
			{
				fnt.NumberOfGlyphs = rdr.ReadUInt16();
			}
			else if (fnt.MaxP_Version == 1.0)
			{
				fnt.NumberOfGlyphs = rdr.ReadUInt16();
				fnt.MaxPoints = rdr.ReadUInt16();
				fnt.MaxContours = rdr.ReadUInt16();
				fnt.MaxCompositePoints = rdr.ReadUInt16();
				fnt.MaxCompositeContours = rdr.ReadUInt16();
				fnt.MaxZones = rdr.ReadUInt16();
				fnt.MaxTwilightPoints = rdr.ReadUInt16();
				fnt.MaxStorage = rdr.ReadUInt16();
				fnt.MaxFunctionDefs = rdr.ReadUInt16();
				fnt.MaxInstructionDefs = rdr.ReadUInt16();
				fnt.MaxStackElements = rdr.ReadUInt16();
				fnt.MaxSizeOfInstructions = rdr.ReadUInt16();
				fnt.MaxComponentElements = rdr.ReadUInt16();
				fnt.MaxComponentDepth = rdr.ReadUInt16();
			}
			else
			{
				throw new Exception("Unknown version for the 'maxp' table!");
			}
			fnt.TableRead_MaxP = true;
		}
	}
}
