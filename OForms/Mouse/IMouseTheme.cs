using System;
using System.Collections.Generic;
using System.Text;
using Orvid.Graphics;

namespace OForms.Mouse
{
	/// <summary>
	/// Represents a theme for a mouse.
	/// </summary>
	public interface IMouseTheme
	{
		/// <summary>
		/// The default cursor.
		/// </summary>
		Image Default { get; }
		/// <summary>
		/// The cursor shown when
		/// resizing horizontally.
		/// </summary>
		Image HResize { get; }
		/// <summary>
		/// The cursor shown when
		/// resizing vertically.
		/// </summary>
		Image VResize { get; }
		/// <summary>
		/// The cursor shown when
		/// resizing diagonally to
		/// the left.
		/// </summary>
		Image DLResize { get; }
		/// <summary>
		/// The cursor shown when
		/// resizing diagonally to
		/// the right.
		/// </summary>
		Image DRResize { get; }
		/// <summary>
		/// The height of the biggest
		/// mouse image this theme
		/// provides.
		/// </summary>
		int MaxHeight { get; }
		/// <summary>
		/// The width of the biggest
		/// mouse image this them
		/// provides.
		/// </summary>
		int MaxWidth { get; }
	}

	/// <summary>
	/// The various types of mouse
	/// cursors that can be used.
	/// </summary>
	public enum MouseType
	{
		Default,
		HResize,
		VResize,
		DLResize,
		DRResize,
	}
}
