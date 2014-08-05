using System;
using System.Collections.Generic;
using System.Text;
using Orvid.Graphics;

namespace OForms
{
	/// <summary>
	/// This interface represents
	/// a class that has a buffer
	/// that can be drawn by a parent.
	/// </summary>
	public interface IBufferedDrawable
	{
		/// <summary>
		/// The class's buffer which
		/// will get drawn.
		/// </summary>
		Image Buffer { get; }
	}
}
