using System;
using Forms = System.Windows.Forms;

namespace TestBed
{
    public static class Utils
    {

        /// <summary>
        /// Converts a System.Windows.Forms.MouseButtons to OForms.MouseButtons
        /// </summary>
        /// <param name="b">Object to convert.</param>
        /// <returns>Converted buttons.</returns>
        public static OForms.Mouse.MouseButtons GetButtons(Forms.MouseButtons b)
        {
			OForms.Mouse.MouseButtons buttons = OForms.Mouse.MouseButtons.None;

            if ((b & Forms.MouseButtons.Left) != 0)
            {
				buttons |= OForms.Mouse.MouseButtons.Left;
            }
			else if ((b & Forms.MouseButtons.Middle) != 0)
            {
				buttons |= OForms.Mouse.MouseButtons.Middle;
            }
			else if ((b & Forms.MouseButtons.Right) != 0)
            {
				buttons |= OForms.Mouse.MouseButtons.Right;
            }
			else if ((b & Forms.MouseButtons.XButton1) != 0)
            {
				buttons |= OForms.Mouse.MouseButtons.XButton1;
            }
			else if ((b & Forms.MouseButtons.XButton2) != 0)
            {
				buttons |= OForms.Mouse.MouseButtons.XButton2;
            }

            return buttons;
        }
    }
}
