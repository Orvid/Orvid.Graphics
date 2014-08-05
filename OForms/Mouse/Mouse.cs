using System;
using System.Collections.Generic;
using System.Text;
using Orvid.Graphics;
using OForms.Mouse;

namespace OForms.Windows
{
	/// <summary>
	/// Represents a mouse on the desktop.
	/// </summary>
	public class Mouse
	{
		/// <summary>
		/// The image of what's behind the
		/// mouse.
		/// </summary>
		private Image behindMouseImage;
		/// <summary>
		/// The offset from the reported
		/// position to where the mouse 
		/// should actually be drawn.
		/// </summary>
		private Vec2 MouseOffset = Vec2.Zero;
		/// <summary>
		/// The location of the mouse.
		/// </summary>
		private Vec2 local_MouseLocation = Vec2.Zero;
		/// <summary>
		/// The location of the mouse.
		/// </summary>
		public Vec2 MouseLocation
		{
			get { return local_MouseLocation + MouseOffset; }
			set
			{
				MouseX = value.X;
				MouseY = value.Y;
			}
		}
		/// <summary>
		/// The X position of the mouse
		/// on the screen.
		/// </summary>
		public int MouseX
		{
			get { return local_MouseLocation.X + MouseOffset.X; }
			set 
			{
				if (value > parent.Size.X)
				{
					local_MouseLocation.X = parent.Size.X - Theme.MaxWidth - 1;
				}
				else
				{
					local_MouseLocation.X = value;
				}
			}
		}
		/// <summary>
		/// The Y position of the mouse
		/// on the screen.
		/// </summary>
		public int MouseY
		{
			get { return local_MouseLocation.Y + MouseOffset.Y; }
			set
			{
				if (value > parent.Size.Y)
				{
					local_MouseLocation.Y = parent.Size.Y - Theme.MaxHeight - 1;
				}
				else
				{
					local_MouseLocation.Y = value;
				}
			}
		}
		/// <summary>
		/// The theme for the mouse cursors.
		/// </summary>
		private IMouseTheme Theme = new OForms.Mouse.Themes.DefaultMouseTheme();
		/// <summary>
		/// The type of mouse to draw.
		/// </summary>
		private MouseType mouseType = MouseType.Default;
		/// <summary>
		/// The current buttons that are pressed.
		/// </summary>
		private MouseButtons pressedButtons = MouseButtons.None;
		/// <summary>
		/// The currently pressed buttons.
		/// </summary>
		public MouseButtons PressedButtons
		{
			get { return pressedButtons; }
		}
		/// <summary>
		/// The <see cref="WindowManager"/> that
		/// created this instance.
		/// </summary>
		private WindowManager parent;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Mouse"/> class.
		/// </summary>
		public Mouse(WindowManager parent)
		{
			this.parent = parent;
			MouseX = 0;
			MouseY = 0;
		}

		/// <summary>
		/// Marks the specified button as
		/// pressed.
		/// </summary>
		/// <param name="button">The button to press.</param>
		public void PressButton(MouseButtons button)
		{
			pressedButtons |= button;
		}

		/// <summary>
		/// Releases the specified button
		/// </summary>
		/// <param name="button">The button to release.</param>
		public void ReleaseButton(MouseButtons button)
		{
			pressedButtons &= ~button;
		}

		/// <summary>
		/// Set the theme of the mouse
		/// cursors to the specified theme.
		/// </summary>
		/// <param name="theme">The theme to use.</param>
		public void SetTheme(IMouseTheme theme)
		{
			this.Theme = theme;
		}

		/// <summary>
		/// Sets the type of mouse to
		/// display.
		/// </summary>
		/// <param name="type">The type of mouse to display.</param>
		public void SetType(MouseType type)
		{
			MouseOffset = Vec2.Zero;
			// Now apply the offset for
			// the target type.
			switch (type)
			{
				case MouseType.Default: break;

				case MouseType.HResize:
					MouseOffset.X = -(this.Theme.HResize.Width >> 1);
					break;

				case MouseType.VResize:
					MouseOffset.Y = -(this.Theme.VResize.Height >> 1);
					break;

				case MouseType.DLResize:
					MouseOffset.Y = -(this.Theme.DLResize.Height >> 1);
					MouseOffset.X = -(this.Theme.DLResize.Width >> 1);
					break;

				case MouseType.DRResize:
					MouseOffset.Y = -(this.Theme.DRResize.Height >> 1);
					MouseOffset.X = -(this.Theme.DRResize.Width >> 1);
					break;

				default:
					throw new Exception("Unknown Mouse Type!");
			}
			this.mouseType = type;
		}

		/// <summary>
		/// Draws the mouse to the specified
		/// image.
		/// </summary>
		/// <param name="img">The image to draw to.</param>
		public void Draw(Image img)
		{
			lock (img)
			{
				switch (this.mouseType)
				{
					case MouseType.Default:
						behindMouseImage = img.SubImage(local_MouseLocation + MouseOffset, Theme.Default.Size);
						img.DrawImage(local_MouseLocation + MouseOffset, Theme.Default);
						break;
					case MouseType.HResize:
						behindMouseImage = img.SubImage(local_MouseLocation + MouseOffset, Theme.HResize.Size);
						img.DrawImage(local_MouseLocation + MouseOffset, Theme.HResize);
						break;
					case MouseType.VResize:
						behindMouseImage = img.SubImage(local_MouseLocation + MouseOffset, Theme.VResize.Size);
						img.DrawImage(local_MouseLocation + MouseOffset, Theme.VResize);
						break;
					case MouseType.DLResize:
						behindMouseImage = img.SubImage(local_MouseLocation + MouseOffset, Theme.DLResize.Size);
						img.DrawImage(local_MouseLocation + MouseOffset, Theme.DLResize);
						break;
					case MouseType.DRResize:
						behindMouseImage = img.SubImage(local_MouseLocation + MouseOffset, Theme.DRResize.Size);
						img.DrawImage(local_MouseLocation + MouseOffset, Theme.DRResize);
						break;
					default:
						throw new Exception("Unknown Mouse Type!");
				}
			}
		}

		/// <summary>
		/// Restores the image behind the
		/// mouse.
		/// </summary>
		/// <param name="img">The image to restore to.</param>
		public void Restore(Image img)
		{
			img.DrawImage(local_MouseLocation + MouseOffset, behindMouseImage);
		}

	}
}
