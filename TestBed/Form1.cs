using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Drawing = System.Drawing;
using System.Text;
using Forms = System.Windows.Forms;
using Orvid.Graphics;
using OForms;
using OForms.Controls;
using OForms.Windows;

namespace TestBed
{
    public partial class Form1 : Forms.Form
    {
        private const int DesktopWidth = 640;
        private const int DesktopHeight = 480;
        List<ObjectEvents> Objects = new List<ObjectEvents>();
        Image i = new Image(DesktopWidth, DesktopHeight);
        WindowManager windowManager = new WindowManager(new Vec2(DesktopWidth, DesktopHeight));
        //Orvid.Graphics.FontSupport.Font fnt;
        Window w1;

        public Form1()
        {
            InitializeComponent();
            //System.IO.StreamReader sr = new System.IO.StreamReader("Vera-10.bdf");
            //Orvid.Graphics.FontSupport.FontManager.Instance.LoadFont(1, sr.BaseStream);
            //System.IO.StreamReader sr = new System.IO.StreamReader("MS-Sans-Serif_24.FNT");
            //fnt = Orvid.Graphics.FontSupport.FontManager.Instance.LoadFont(2, sr.BaseStream);
		}

		void Form1_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
		{
			windowManager.Shutdown();
		}

        void bt_Click(Vec2 loc, OForms.Mouse.MouseButtons buttons)
        {
            //if (w1.CurrentState == OForms.Windows.WindowState.Normal)
            //{
            //    windowManager.MaximizeWindow(w1);
            //}
            //else
            //{
            //    windowManager.RestoreWindow(w1);
            //}
            Window w = new Window(new Vec2(130, 30), new Vec2(120, 80), "Test Window 3");
            w.ClearColor = Colors.Blue;
            windowManager.AddWindow(w);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            w1 = new Window(new Vec2(30, 30), new Vec2(120, 80), "Test Window 1");
            w1.ClearColor = Colors.Green;
            Button bt = new Button(new Vec2(10, 10), new Vec2(90, 15));
            bt.Click += new MouseEvent(bt_Click);
            bt.Parent = w1;
			bt.Text = "Create New Window";
            w1.Controls.Add(bt);
            Window w2 = new Window(new Vec2(80, 30), new Vec2(120, 80), "Test Window 2");
            w2.ClearColor = Colors.Red;
            Window w3 = new Window(new Vec2(130, 30), new Vec2(120, 80), "Test Window 3");
            w3.ClearColor = Colors.Blue;
            windowManager.AddWindow(w1);
            windowManager.AddWindow(w2);
            windowManager.AddWindow(w3);
            windowManager.BringWindowToFront(w1);

            Forms.Cursor.Hide();

            i.Clear(Colors.White);

            //i.DrawString(new Vec2(30, 30), "T", fnt, 20, Orvid.Graphics.FontSupport.FontStyle.Normal, Colors.Black);

            // Draw exit button.
            ExitButton b = new ExitButton(new Vec2(DesktopWidth - 21, 1), new Vec2(20, 20), i, this);
            Objects.Add(b.evnts);


			windowManager.DrawMouse(i);
            pictureBox1.Image = (System.Drawing.Bitmap)i;
			pictureBox1.Refresh();
			windowManager.UnDrawMouse(i);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            foreach (ObjectEvents o in Objects)
            {
				if (o.Bounds.IsInBounds(new Vec2(windowManager.Mouse.MouseX, windowManager.Mouse.MouseY)))
                {
					o.MouseClick(sender, new Forms.MouseEventArgs(Forms.MouseButtons.Left, 1, windowManager.Mouse.MouseX, windowManager.Mouse.MouseY, 0));
                }
            }

			windowManager.HandleMouseClick(new Vec2(windowManager.Mouse.MouseX, windowManager.Mouse.MouseY), OForms.Mouse.MouseButtons.Left, i);

			windowManager.DrawMouse(i);
            pictureBox1.Image = (System.Drawing.Bitmap)i;
			pictureBox1.Refresh();
			windowManager.UnDrawMouse(i);
        }

        private void pictureBox1_MouseMove(object sender, Forms.MouseEventArgs e)
        {

            windowManager.HandleMouseMove(new Vec2(e.X, e.Y), Utils.GetButtons(e.Button), i);


            foreach (ObjectEvents c in Objects)
            {
                if (!c.IsIn)
                {
                    if (c.Bounds.IsInBounds(new Vec2(e.X, e.Y)))
                    {
                        c.IsIn = true;
                        if (c.IsMouseDown)
                        {
							c.MouseDown(sender, new Forms.MouseEventArgs(Forms.MouseButtons.Left, 1, windowManager.Mouse.MouseX, windowManager.Mouse.MouseY, 0));
                        }
                        else
                        {
							c.MouseEnter(sender, new Forms.MouseEventArgs(Forms.MouseButtons.Left, 1, windowManager.Mouse.MouseX, windowManager.Mouse.MouseY, 0));
                        }
                    }
                }
                else
                {
                    if (!c.Bounds.IsInBounds(new Vec2(e.X, e.Y)))
                    {
                        c.IsIn = false;
						c.MouseLeave(sender, new Forms.MouseEventArgs(Forms.MouseButtons.Left, 1, windowManager.Mouse.MouseX, windowManager.Mouse.MouseY, 0));
                    }
                }
            }

			windowManager.DrawMouse(i);
            pictureBox1.Image = (System.Drawing.Bitmap)i;
			pictureBox1.Refresh();
			windowManager.UnDrawMouse(i);
        }

        private void pictureBox1_MouseDown(object sender, Forms.MouseEventArgs e)
        {
            windowManager.HandleMouseDown(new Vec2(e.X, e.Y), Utils.GetButtons(e.Button), i);
            
            foreach (ObjectEvents c in Objects)
            {
                if (c.Bounds.IsInBounds(new Vec2(e.X, e.Y)))
                {
                    c.IsMouseDown = true;
					c.MouseDown(sender, new Forms.MouseEventArgs(Forms.MouseButtons.Left, 1, windowManager.Mouse.MouseX, windowManager.Mouse.MouseY, 0));
                }
            }

			windowManager.DrawMouse(i);
            pictureBox1.Image = (System.Drawing.Bitmap)i;
			pictureBox1.Refresh();
			windowManager.UnDrawMouse(i);
        }

        private void pictureBox1_MouseUp(object sender, Forms.MouseEventArgs e)
        {
            windowManager.HandleMouseUp(new Vec2(e.X, e.Y), Utils.GetButtons(e.Button), i);

            foreach (ObjectEvents c in Objects)
            {
                if (c.IsMouseDown)
                {
                    c.IsMouseDown = false;
					c.MouseUp(sender, new Forms.MouseEventArgs(Forms.MouseButtons.Left, 1, windowManager.Mouse.MouseX, windowManager.Mouse.MouseY, 0));
                }
            }

			windowManager.DrawMouse(i);
            pictureBox1.Image = (System.Drawing.Bitmap)i;
			pictureBox1.Refresh();
			windowManager.UnDrawMouse(i);
        }

    }

}
