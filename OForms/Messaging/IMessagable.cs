using System;
using System.Collections.Generic;
using System.Text;

namespace OForms.Messaging
{
	/// <summary>
	/// Represents a class that can
	/// recieve messages.
	/// </summary>
	public interface IMessagable
	{
		/// <summary>
		/// Sends the specified message.
		/// </summary>
		/// <param name="msg">The message to send.</param>
		void SendMessage(Message msg);
	}
}
