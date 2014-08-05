using System;
using Orvid.Graphics;
using OForms.Mouse;
using Orvid.Graphics.FontSupport;

namespace OForms.Controls
{
    /// <summary>
    /// A button.
    /// </summary>
    public class Button : Control
    {
        /// <summary>
        /// The internal size of the button. This is slighly smaller
        /// than the size that was specified, as the buffer has a base of 0,
        /// where-as the size has a base of 1.
        /// </summary>
        private Vec2 iSize;

		private Font local_TextFont = Windows.WindowManager.WindowFont;
		/// <summary>
		/// The font to draw 
		/// the text in.
		/// </summary>
		public Font TextFont
		{
			get { return local_TextFont; }
			set { local_TextFont = value ?? Windows.WindowManager.WindowFont; }
		}

        /// <summary>
        /// The default constructor for a button.
        /// </summary>
        /// <param name="loc">The location of the button in the parent control.</param>
        /// <param name="size">The size of the button.</param>
        public Button(Vec2 loc, Vec2 size)
            : base(loc, size)
        {
            if (size.X == 0 || size.Y == 0)
            {
                throw new Exception("No dimention of size can be zero!");
            }
            this.X = loc.X;
            this.Y = loc.Y;
            this.Size = size;
            this.iSize = new Vec2(size.X - 1, size.Y - 1);
			CalculateBounds();
            MouseEnter += new MouseEvent(this.ButtonEnter);
            MouseLeave += new MouseEvent(this.ButtonLeave);
            MouseDown += new MouseEvent(this.ButtonMouseDown);
            MouseUp += new MouseEvent(this.ButtonMouseUp);
			SizeChanged += new EventHandler(Changed);
			Move += new EventHandler(Changed);
			TextChanged += new EventHandler(Changed);
			Buffer = new Image(size);
            this.DrawDefault();
        }

		/// <summary>
		/// Used to ensure the bounds and
		/// similar things are accurate.
		/// </summary>
		private void Changed(object sender, EventArgs args)
		{
			CalculateBounds();
			DrawDefault();
		}

		/// <summary>
		/// Calculates all of the
		/// button's bounds.
		/// </summary>
		private void CalculateBounds()
		{
			this.Bounds = new BoundingBox(this.X, this.X + Size.X, this.Y + Size.Y, this.Y);
		}

        /// <summary>
        /// Draws the button on the specified image.
        /// </summary>
        /// <param name="i">The image to draw the button on.</param>
        public override void Draw(Image i)
        {
            i.DrawImage(new Vec2(X, Y), Buffer);
        }

        #region Event Methods

        private void ButtonMouseUp(Vec2 loc, MouseButtons buttons)
        {
			this.DrawDefault();
        }

        private void ButtonMouseDown(Vec2 loc, MouseButtons buttons)
        {
            this.DrawMouseDown();
        }

        private void ButtonEnter(Vec2 loc, MouseButtons buttons)
        {
            if (IsMouseDown)
            {
                this.DrawMouseDown();
            }
            else
            {
                this.DrawDefault();
            }
        }

        private void ButtonLeave(Vec2 loc, MouseButtons buttons)
        {
            this.DrawDefault();
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// The fill of the button in it's default state.
        /// </summary>
		public Pixel Background = CustomColors.Control;

		/// <summary>
		/// Draws the button in the 'Down' state.
		/// </summary>
		private void DrawMouseDown()
        {
			Buffer.DrawRectangle(Vec2.Zero, new Vec2(iSize.X, Size.Y), Background);

			this.DrawString(new Vec2(3, 7));

			Buffer.DrawLine(Vec2.Zero, new Vec2(iSize.X, 0), CustomColors.ControlDarkDark);
			Buffer.DrawLine(Vec2.Zero, new Vec2(0, iSize.Y), CustomColors.ControlDarkDark);
			Buffer.DrawLine(new Vec2(1, 1), new Vec2(1, iSize.Y - 2), CustomColors.ControlDark);
			Buffer.DrawLine(new Vec2(1, 1), new Vec2(iSize.X - 2, 1), CustomColors.ControlDark);
			Buffer.DrawLine(new Vec2(iSize.X - 1, 1), new Vec2(iSize.X - 1, iSize.Y - 1), CustomColors.Control);
			Buffer.DrawLine(new Vec2(1, iSize.Y - 1), new Vec2(iSize.X - 1, iSize.Y - 1), CustomColors.Control);
			Buffer.DrawLine(new Vec2(0, iSize.Y), new Vec2(iSize.X, iSize.Y), CustomColors.ControlLightLight);
			Buffer.DrawLine(new Vec2(iSize.X, 0), new Vec2(iSize.X, iSize.Y), CustomColors.ControlLightLight);
        }

		/// <summary>
		/// Draws the current value of 
		/// Text to the buffer.
		/// </summary>
		private void DrawString(Vec2 offset)
		{
			BoundingBox bnds = this.Bounds;
			Vec2 sz = this.iSize - offset;
			if (TextFont.GetFontMetrics().StringWidth(this.Text) > sz.X - 6)
			{
				// Doesn't fit on the button, need to remove some characters.
				string s = this.Text.Substring(0, this.Text.Length - 3) + "...";
				while (TextFont.GetFontMetrics().StringWidth(s) > sz.X - 6)
				{
					if (s.Length == 3)
					{
						s = "."; // button is to small to have a name drawn.
						break;
					}
					else
					{
						// It's 4 to make up for the 3 extra characters we add.
						s = s.Substring(0, s.Length - 4) + "...";
					}
				}
				Buffer.DrawString(
					new Vec2(bnds.Left - offset.X, bnds.Bottom - offset.Y), 
					s,
					TextFont,
					10,
					FontStyle.Normal,
					CustomColors.ControlText
				);
			}
			else // Fits on button.
			{
				Buffer.DrawString(
					new Vec2(bnds.Left + offset.X, bnds.Bottom + offset.Y), 
					this.Text,
					TextFont, 
					10, 
					FontStyle.Normal, 
					CustomColors.ControlText
				);
			}
		}

        /// <summary>
        /// Draws the button in it's default state.
        /// </summary>
        private void DrawDefault()
        {
			Buffer.DrawRectangle(new Vec2(0, 0), new Vec2(iSize.X, iSize.Y), Background);

			this.DrawString(new Vec2(4, 8));

			Buffer.DrawLine(Vec2.Zero, new Vec2(iSize.X, 0), CustomColors.ControlLightLight);
			Buffer.DrawLine(Vec2.Zero, new Vec2(0, iSize.Y), CustomColors.ControlLightLight);
			Buffer.DrawLine(new Vec2(iSize.X - 1, 1), new Vec2(iSize.X - 1, iSize.Y - 1), CustomColors.ControlDark);
			Buffer.DrawLine(new Vec2(1, iSize.Y - 1), new Vec2(iSize.X - 1, iSize.Y - 1), CustomColors.ControlDark);
			Buffer.DrawLine(new Vec2(0, iSize.Y), new Vec2(iSize.X, iSize.Y), CustomColors.ControlDarkDark);
			Buffer.DrawLine(new Vec2(iSize.X, 0), new Vec2(iSize.X, iSize.Y), CustomColors.ControlDarkDark);
        }
        #endregion

    }
}
