using System;
using System.Collections.Generic;
using System.Text;

namespace OForms.Windows
{
	/// <summary>
	/// The types of resizing
	/// that it's possible to 
	/// perform.
	/// </summary>
	internal enum ResizeType
	{
		/// <summary>
		/// No resize is being
		/// performed.
		/// </summary>
		None,
		/// <summary>
		/// The resize is
		/// ocurring from the
		/// top left.
		/// </summary>
		TopLeft,
		/// <summary>
		/// The resize is
		/// ocurring from the
		/// top.
		/// </summary>
		Top,
		/// <summary>
		/// The resize is
		/// ocurring from the
		/// top right.
		/// </summary>
		TopRight,
		/// <summary>
		/// The resize is
		/// ocurring from the
		/// right.
		/// </summary>
		Right,
		/// <summary>
		/// The resize is
		/// ocurring from the
		/// bottom right.
		/// </summary>
		BottomRight,
		/// <summary>
		/// The resize is
		/// ocurring from the
		/// bottom.
		/// </summary>
		Bottom,
		/// <summary>
		/// The resize is
		/// ocurring from the
		/// bottom left.
		/// </summary>
		BottomLeft,
		/// <summary>
		/// The resize is
		/// ocurring from the
		/// left.
		/// </summary>
		Left
	}
}
