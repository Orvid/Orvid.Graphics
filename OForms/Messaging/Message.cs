using System;
using System.Collections.Generic;
using System.Text;

namespace OForms.Messaging
{
	/// <summary>
	/// Represents a single message
	/// that is being sent to an IMessagable.
	/// </summary>
	public struct Message
	{
		#region Constants
		/// <summary>
		/// This message should never
		/// be sent nor recieved.
		/// </summary>
		public const int MSG_UNKNOWN = 0;
		/// <summary>
		/// This message is sent to a 
		/// window which is requested
		/// to close.
		/// </summary>
		/// <remarks>
		/// The data value is a 32-bit
		/// signed integer representing
		/// the window's ID.
		/// </remarks>
		public const int MSG_EXIT = 1;
		/// <summary>
		/// This message is sent to a
		/// window to ensure that it's
		/// responsive.
		/// </summary>
		/// <remarks>
		/// This should be responded
		/// to by sending the window
		/// manager a <see cref="MSG_PONG"/>.
		/// 
		/// <para></para>
		/// <para>
		/// The data value is a 32-bit
		/// signed integer representing
		/// the window's ID.
		/// </para>
		/// 
		/// </remarks>
		public const int MSG_PING = 2;
		/// <summary>
		/// This message is sent by a 
		/// window to tell the window
		/// manager that it's responsive.
		/// </summary>
		/// <remarks>
		/// This should only be sent in 
		/// response to a <see cref="MSG_PING"/>
		/// message.
		/// 
		/// <para></para>
		/// <para>
		/// The data value is a 32-bit
		/// signed integer representing
		/// the window's ID.
		/// </para>
		/// 
		/// </remarks>
		public const int MSG_PONG = 3;
		/// <summary>
		/// This message is sent to
		/// the window manager to tell
		/// it to shutdown.
		/// </summary>
		public const int MSG_SHUTDOWN = 4;
		/// <summary>
		/// This message is sent to the
		/// window manager by a window
		/// to tell it to re-draw. It is
		/// also sent to a window by
		/// the window manager to tell a
		/// window it needs to re-draw.
		/// </summary>
		/// <remarks>
		/// The data value is a 32-bit
		/// signed integer representing
		/// the window's ID.
		/// </remarks>
		public const int MSG_DRAW = 5;
		/// <summary>
		/// This message is sent to a
		/// window to tell it the mouse
		/// has been clicked.
		/// </summary>
		/// <remarks>
		/// The data value is a set of 3
		/// 32-bit signed integer representing
		/// the X position, the Y position, 
		/// and the buttons pressed, in that
		/// order.
		/// </remarks>
		public const int MSG_CLICK = 6;
		/// <summary>
		/// This message is sent to a
		/// window to tell it the mouse
		/// has been moved.
		/// </summary>
		/// <remarks>
		/// The data value is a set of 2
		/// 32-bit signed integer representing
		/// the X position, the Y position,
		/// and the buttons pressed, in that
		/// order.
		/// </remarks>
		public const int MSG_MOUSE_MOVE = 7;
		/// <summary>
		/// This message is sent to a
		/// window to tell it that a mouse
		/// button has been pressed.
		/// </summary>
		/// <remarks>
		/// The data value is a set of 2
		/// 32-bit signed integer representing
		/// the X position, the Y position,
		/// and the buttons pressed, in that
		/// order.
		/// </remarks>
		public const int MSG_MOUSE_DOWN = 8;
		/// <summary>
		/// This message is sent to a
		/// window to tell it that a mouse
		/// button has been released.
		/// </summary>
		/// <remarks>
		/// The data value is a set of 2
		/// 32-bit signed integer representing
		/// the X position, the Y position,
		/// and the buttons pressed, in that
		/// order.
		/// </remarks>
		public const int MSG_MOUSE_UP = 9;

		#endregion


		private int msgType;
		private byte[] dat;
		/// <summary>
		/// The type of message
		/// this is.
		/// </summary>
		public int MessageType { get { return msgType; } }
		/// <summary>
		/// The actual data of this message.
		/// </summary>
		public byte[] Data { get { return dat; } }

		/// <summary>
		/// Creates a new instance of the 
		/// <see cref="Message"/> class.
		/// </summary>
		/// <param name="messageType">
		/// The type of message this is.
		/// </param>
		public Message(int messageType)
		{
			this.msgType = messageType;
			this.dat = new byte[0];
		}

		/// <summary>
		/// Creates a new instance of the 
		/// <see cref="Message"/> class.
		/// </summary>
		/// <param name="messageType">
		/// The type of message this is.
		/// </param>
		/// <param name="data">
		/// The data of this message.
		/// </param>
		public Message(int messageType, byte[] data)
		{
			this.msgType = messageType;
			this.dat = data;
		}
	}
}
