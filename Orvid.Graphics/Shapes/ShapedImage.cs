﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics.Shapes
{
    public class ShapedImage : Image
    {
		private bool local_Modified;
        public bool Modified
		{
			get { return local_Modified; }
			set { local_Modified = value; }
		}
        public List<Shape> Shapes = new List<Shape>();

        public ShapedImage(int width, int height) : base(width, height)
        {
        }

        public Image Render()
        {
            if (Modified)
            {
                this.Clear(new Pixel(true));
                foreach (Shape s in Shapes)
                {
                    s.Draw();
                }
                this.Modified = false;
            }
            return this;
        }
    }
}
