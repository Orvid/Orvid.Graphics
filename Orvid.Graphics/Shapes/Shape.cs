﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics.Shapes
{
    public abstract class Shape
    {
        private int x;
        public Int32 X 
        {
            get
            {
                return x;
            }
            set
            {
                Modified = true;
                x = value;
            }
        }
        private int y;
        public Int32 Y
        {
            get
            {
                return y;
            }
            set
            {
                Modified = true;
                y = value;
            }
        }
        private bool modified = true;
        public bool Modified
        {
            get
            {
                return modified;
            }
            internal set
            {
                modified = value;
                if (modified)
                {
                    Parent.Modified = true;
                }
            }
        }
		private ShapedImage local_Parent;
        public ShapedImage Parent 
		{
			get { return local_Parent; }
			set { local_Parent = value; }
		}
        public abstract void Draw();
    }
}
