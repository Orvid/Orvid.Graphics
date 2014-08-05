#define RunTest
#define Profile
#define PreCallForJIT
//#define EmitAssembly
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Orvid.Compiler.TrueType;

namespace TestBed
{
	public partial class TTFLoadingTestForm : Form
	{
		private PictureBox OGraphicsBox;
		private PictureBox ReferenceBox;

		public TTFLoadingTestForm()
		{
			InitializeComponent();
		}

		private void TTFLoadingTestForm_Load(object sender, EventArgs e)
		{
			Bitmap b = new Bitmap(130, 160);
			Graphics g = Graphics.FromImage(b);
			g.Clear(Color.White);
			PrivateFontCollection fonCol = new PrivateFontCollection();
			fonCol.AddFontFile("TimesNewRoman.ttf");
			//g.DrawBezier(Pens.Black,
			//    new Point(52, 79), new Point(61, 76), new Point(70, 66), new Point(70, 59)
			//);
			Stopwatch st = new Stopwatch();
			st.Start();
			g.DrawString("HELLO!", new Font(fonCol.Families[0], (float)RenderSize), Brushes.Black, new PointF(10, 10));
			st.Stop();
			Console.WriteLine("Time to render (GDI+): " + st.ElapsedMilliseconds + " milliseconds");
			fonCol.Dispose();
			ReferenceBox.Image = b;

			#region Create the Font

			FileStream strm = new FileStream("TimesNewRoman2.ttf", FileMode.Open);
			byte[] buf = new byte[strm.Length];
			strm.Read(buf, 0, (int)strm.Length);
			strm.Close();
			MemoryStream memStrm = new MemoryStream(buf);
			trueTypeFont.Load(memStrm);

#if EmitAssembly
			trueTypeFont.Save(AssemblyName, EmitDebugInfo);
#endif

			#endregion

			TestButton.PerformClick();
		}

		private const string AssemblyName = "Test3.dll";
		private const bool EmitDebugInfo = true;
		private const double RenderSize = 10;
		private TrueTypeFont trueTypeFont = new TrueTypeFont();
		private void TestButton_Click(object sender, EventArgs e)
		{

#if RunTest
			Orvid.TrueType.GraphicsState gState = new Orvid.TrueType.GraphicsState();
			gState.SizeInPoints = (uint)RenderSize;
			gState.UnitsPerEm = 2048;
			gState.CalculateScale();

			TrueType.FontDescriptor fdesc = new TrueType.FontDescriptor();
			fdesc.InitializeGraphicsState(gState);
			Orvid.TrueType.Glyph g = fdesc.GetGlyph((int)'H');
			gState.SetDefaults();
			TrueType.Fpgm.Function_0(gState);
			g.InitializeGraphicsState(gState);
#if Profile

#if PreCallForJIT
			const int InitCallCount = 1;
			for (int i = 0; i < InitCallCount; i++)
			{
			    TrueType.Prep.InitializeState(gState);
			}
			gState.SetDefaultsForGlyphs();
			g.Hint(gState);
			OGraphicsBox.Image = (Image)(Orvid.Graphics.Image)g.GetRendering(RenderSize, gState);
			g.InitializeGraphicsState(gState);
#endif

			// The first run through of it takes
			// about 500ms on a 0.5GFlop machine. (vs. 1ms)
			// (Probably JITing time)
			Stopwatch st = new Stopwatch();
			const int CallCount = 20;
			for (int i = 0; i < CallCount; i++)
			{
				System.GC.Collect();
				System.GC.WaitForFullGCComplete();
				System.GC.WaitForPendingFinalizers();
				st.Start();
				TrueType.Prep.InitializeState(gState);
				st.Stop();
			}
			long initStateTime = st.ElapsedTicks / CallCount;
			long initStateTimeMS = st.ElapsedMilliseconds / CallCount;
			st.Reset();

			st.Start();
			gState.SetDefaultsForGlyphs();
			st.Stop();
			long defSetTime = st.ElapsedTicks;
			long defSetTimeMS = st.ElapsedMilliseconds;
			st.Reset();

			// Unfortunately, glyph hinting isn't
			// designed to run  repeatidly.
			st.Start();
			g.Hint(gState);
			st.Stop();
			long gHintTime = st.ElapsedTicks;
			long gHintTimeMS = st.ElapsedMilliseconds;
			st.Reset();

			//long prevMS = 0;
			//long prevTK = 0;
			for (int i = 0; i < CallCount; i++)
			{
				System.GC.Collect();
				System.GC.WaitForFullGCComplete();
				System.GC.WaitForPendingFinalizers();
				st.Start();
				OGraphicsBox.Image = (Image)(Orvid.Graphics.Image)g.GetRendering(RenderSize, gState);
				st.Stop();
				//Console.WriteLine("Overall rendering took " + (st.ElapsedTicks - prevTK).ToString() + " ticks (" + (st.ElapsedMilliseconds - prevMS).ToString() + "ms).");
				//prevMS = st.ElapsedMilliseconds;
				//prevTK = st.ElapsedTicks;
			}
			long renderTime = st.ElapsedTicks / CallCount;
			long renderTimeMS = st.ElapsedMilliseconds / CallCount;

			MessageBox.Show("It took " + initStateTime.ToString() + " ticks (" + initStateTimeMS.ToString() + "ms) to initialize the graphics state, " + defSetTime.ToString() + " ticks (" + defSetTimeMS.ToString() + "ms) to set defaults, " + gHintTime.ToString() + " ticks (" + gHintTimeMS.ToString() + "ms) to hint the glyph, and " + renderTime.ToString() + " ticks (" + renderTimeMS.ToString() + "ms) to render.");
		
#else
			TrueType.Prep.InitializeState(gState);
			gState.SetDefaultsForGlyphs();
			g.Hint(gState);
			//Orvid.TrueType.Renderer r = new Orvid.TrueType.Renderer();
			//r.TestMe();
			OGraphicsBox.Image = (Image)(Orvid.Graphics.Image)g.GetRendering(RenderSize, gState);
#endif

#endif


			//int i20 = 0;
			//OGraphicsBox.Image.Save("TestRender.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
		}
	}
}