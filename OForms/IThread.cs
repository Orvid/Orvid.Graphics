using System;
using System.Collections.Generic;
using System.Text;

namespace OForms
{
	/// <summary>
	/// Represents a class that will
	/// be run in it's own thread.
	/// </summary>
	public interface IThread
	{
		/// <summary>
		/// Runs the actual thread.
		/// </summary>
		/// <param name="args">The arguments to pass to the thread.</param>
		/// <returns>0 if no error ocurred.</returns>
		int Run(string[] args);
	}
}
