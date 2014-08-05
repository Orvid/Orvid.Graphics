using System;
using Orvid.Graphics;
using System.Collections.Generic;
using Orvid.Graphics.FontSupport;
using OForms.Mouse;
using OForms.Messaging;

namespace OForms.Windows
{
	/// <summary>
	/// The class that represents a WindowManager.
	/// </summary>
	public class WindowManager : IMessagable
	{
		/// <summary>
		/// The class that handles
		/// all messages to and from
		/// the window manager.
		/// </summary>
		private class MessageManager : IMessagable
		{
			/// <summary>
			/// The message queue.
			/// </summary>
			private LinkedQueue<Message> MessageQueue = new LinkedQueue<Message>();
			/// <summary>
			/// The parent.
			/// </summary>
			private WindowManager Parent;


			/// <summary>
			/// A dictionary containing
			/// the windows that we have
			/// told to draw so far.
			/// </summary>
			private Dictionary<int, Window> WindowsToldToDraw;
			/// <summary>
			/// A list of the windows we're
			/// still waiting for a pong from.
			/// </summary>
			private Dictionary<int, Window> WindowsWaitingPongFrom;

			/// <summary>
			/// Creates a new instance of the
			/// <see cref="MessageManager"/> class.
			/// </summary>
			public MessageManager()
			{
				WindowsToldToDraw = new Dictionary<int, Window>();
				WindowsWaitingPongFrom = new Dictionary<int, Window>();

			}

			/// <summary>
			/// Starts up the message manager.
			/// </summary>
			public void Start(WindowManager mngr)
			{
				this.Parent = mngr;
				System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(Run));
				t.Name = "The Window Manager's Message Manager";
				t.Start();
			}

			/// <summary>
			/// Sends a message to the
			/// specified window that tells
			/// it that it needs to re-draw.
			/// </summary>
			/// <param name="wnd">
			/// The window to send the message to.
			/// </param>
			public void SendDraw(Window wnd)
			{
				if (!WindowsToldToDraw.ContainsKey(wnd.ID))
				{
					WindowsToldToDraw.Add(wnd.ID, wnd);
					wnd.SendMessage(new Message(Message.MSG_DRAW, BitConverter.GetBytes(wnd.ID)));
				}
			}

			/// <summary>
			/// Send a ping message to the
			/// specified window.
			/// </summary>
			/// <param name="wnd">The window to ping.</param>
			public void PingWindow(Window wnd)
			{
				if (!WindowsWaitingPongFrom.ContainsKey(wnd.ID))
				{
					WindowsWaitingPongFrom.Add(wnd.ID, wnd);
					wnd.SendMessage(new Message(Message.MSG_PING, BitConverter.GetBytes(wnd.ID)));
				}
			}

			/// <summary>
			/// The function of this method
			/// is as described in <see cref="IMessagable"/>.
			/// </summary>
			public void SendMessage(Message msg)
			{
				MessageQueue.Enqueue(msg);
			}

			#region The Main Loop
			/// <summary>
			/// This is the main
			/// loop for the message
			/// manager.
			/// </summary>
			private void Run()
			{
				Message msg;
				while (true)
				{
					if (MessageQueue.Count > 0)
					{
						msg = MessageQueue.Dequeue();
						switch (msg.MessageType)
						{
							case Message.MSG_SHUTDOWN:
								{
									Console.WriteLine("Message Manager shutting down. {0} item(s) still in the queue.", MessageQueue.Count);
									return;
								}
							case Message.MSG_EXIT:
								{
									int i = BitConverter.ToInt32(msg.Data, 0);
									Window w = Parent.AllWindows[i];
									Parent.RemoveWindow(w);
									Parent.Taskbar.RemoveWindow(w);
									Parent.NeedToRedrawAll = true;
									break;
								}
							case Message.MSG_PONG:
								{
									int windId = BitConverter.ToInt32(msg.Data, 0);
									WindowsWaitingPongFrom.Remove(windId);
									break;
								}
							case Message.MSG_DRAW:
								{
									int windID = BitConverter.ToInt32(msg.Data, 0);
									Window w = Parent.AllWindows[windID];
									if (WindowsToldToDraw.ContainsKey(windID))
									{
										WindowsToldToDraw.Remove(windID);
									}

									if (!w.IsActiveWindow)
									{
										Parent.NeedToRedrawAll = true;
									}
									break;
								}
						}
					}
					else
					{
						System.Threading.Thread.Sleep(5);
					}
				}
			}
			#endregion


		}

		/// <summary>
		/// The message manager.
		/// </summary>
		private MessageManager msgMan = new MessageManager();

		/// <summary>
		/// The function of this method
		/// is as described in <see cref="IMessagable"/>.
		/// </summary>
		public void SendMessage(Message msg)
		{
			msgMan.SendMessage(msg);
		}
		/// <summary>
		/// The height of the taskbar.
		/// </summary>
		internal const int TaskBarHeight = Taskbar.TaskBarHeight;

		/// <summary>
		/// Shuts down the window
		/// manager.
		/// </summary>
		public void Shutdown()
		{
			while (Windows.Length > 0)
			{
				CloseWindow(Windows[0]);
			}
			msgMan.SendMessage(new Message(Message.MSG_SHUTDOWN));
		}

		internal static Font WindowFont = FontManager.Instance.LoadFont(0, new System.IO.MemoryStream(EmbeddedFiles.Fonts.Vera10_bdf));
		/// <summary>
		/// The mouse.
		/// </summary>
		private Mouse theMouse;
		/// <summary>
		/// The mouse.
		/// </summary>
		public Mouse Mouse
		{
			get { return theMouse; }
		}
		/// <summary>
		/// The taskbar.
		/// </summary>
		private Taskbar Taskbar;
		/// <summary>
		/// Is true when all the windows need to be re-drawn,
		/// in other-words, is true if a window has been moved,
		/// resized, added, or removed.
		/// </summary>
		internal bool NeedToRedrawAll = false;
		/// <summary>
		/// An array containing all of the active windows 
		/// in the current window manager instance.
		/// </summary>
		public Window[] Windows;
		/// <summary>
		/// The currently active window. Beware,
		/// there is no array bounds check.
		/// </summary>
		public Window ActiveWindow
		{
			get
			{
				return Windows[0];
			}
			set
			{
				BringWindowToFront(value);
			}
		}
		/// <summary>
		/// The size of the screen.
		/// </summary>
		public Vec2 Size;

		/// <summary>
		/// The default constructor.
		/// </summary>
		public WindowManager(Vec2 size)
		{
			Windows = new Window[0];
			this.Size = size;
			this.Taskbar = new Taskbar(this);
			theMouse = new Mouse(this);
			this.msgMan.Start(this);
		}

		/// <summary>
		/// Draws all the windows on the specified image.
		/// </summary>
		/// <param name="i">The image to draw the windows on.</param>
		public void Draw(Image i)
		{
			lock (Windows)
			{
				if (NeedToRedrawAll)
				{
					i.Clear(Colors.White);
					if (Windows.Length > 0 && Windows[0].CurrentState == WindowState.Maximized)
					{
						i.DrawImage(Windows[0].Location, Windows[0].Buffer);
						this.msgMan.SendDraw(Windows[0]);
					}
					else
					{
						for (int ind = Windows.Length - 1; ind >= 0; ind--)
						{
							if (Windows[ind].CurrentState != WindowState.Minimized)
							{
								i.DrawImage(Windows[ind].Location, Windows[ind].Buffer);
								this.msgMan.SendDraw(Windows[ind]);
							}
						}
					}
					NeedToRedrawAll = false;
				}
				else
				{
					if (Windows.Length > 0)
					{
						if (Windows[0].CurrentState != WindowState.Minimized)
						{
							i.DrawImage(Windows[0].Location, Windows[0].Buffer);
							this.msgMan.SendDraw(Windows[0]);
						}
					}
				}
			}
			Taskbar.Draw(i);
		}

		/// <summary>
		/// Draws the mouse on the
		/// specified image.
		/// </summary>
		/// <param name="i">The i</param>
		public void DrawMouse(Image i)
		{
			theMouse.Draw(i);
		}

		/// <summary>
		/// Restores the image behind
		/// the mouse.
		/// </summary>
		/// <param name="img">The image to restore onto.</param>
		public void UnDrawMouse(Image img)
		{
			theMouse.Restore(img);
		}

		private Dictionary<int, Window> AllWindows = new Dictionary<int, Window>();

		/// <summary>
		/// Adds a window at the front.
		/// </summary>
		/// <param name="w">The window to add.</param>
		public void AddWindow(Window w)
		{
			// This ensures that the
			// window has a unique ID.
			while (AllWindows.ContainsKey(w.ID))
			{
				Window.SetWindowID(w);
			}

			if (Windows.Length > 0)
			{
				Window wnd = Windows[0];
				InternalAddWindow(w);
				msgMan.SendDraw(w);
				msgMan.SendDraw(wnd);
			}
			else
			{
				InternalAddWindow(w);
				msgMan.SendDraw(w);
			}
			Taskbar.AddWindow(w);

			System.Threading.Thread t = new System.Threading.Thread(delegate()
			{
				w.Run(new string[] { });
			});
			t.Start();
		}

		/// <summary>
		/// Adds the specified window without modifying the taskbar.
		/// </summary>
		/// <param name="w">The window to add.</param>
		private void InternalAddWindow(Window w)
		{
			if (AllWindows.ContainsKey(w.ID))
			{
				throw new Exception("Attempted to add a duplicate window!");
			}
			else
			{
				// Now add the window in.
				AllWindows.Add(w.ID, w);
			}
			w.Parent = this;
			w.IsActiveWindow = true;
			if (Windows.Length > 0)
			{
				Windows[0].IsActiveWindow = false;
			}
			Window[] tmp = new Window[Windows.Length + 1];
			Array.Copy(Windows, 0, tmp, 1, Windows.Length);
			tmp[0] = w;
			Windows = tmp;
			NeedToRedrawAll = true;
		}

		/// <summary>
		/// Maximize the specified window.
		/// </summary>
		/// <param name="w">The window to maximize.</param>
		public void MaximizeWindow(Window w)
		{
			w.CurrentState = WindowState.Maximized;
			msgMan.SendDraw(w);
		}

		/// <summary>
		/// Restore a window to the Normal state.
		/// </summary>
		/// <param name="w">The window to restore.</param>
		public void RestoreWindow(Window w)
		{
			w.CurrentState = WindowState.Normal;
			msgMan.SendDraw(w);
		}

		/// <summary>
		/// Minimizes the specified window.
		/// </summary>
		/// <param name="w">The window to minimize.</param>
		public void MinimizeWindow(Window w)
		{
			if (w.IsActiveWindow)
			{
				this.SendWindowToBack(w);
			}
			w.CurrentState = WindowState.Minimized;
			msgMan.SendDraw(w);
		}

		/// <summary>
		/// Sends the specified window to the back.
		/// </summary>
		/// <param name="w">The window to send to the back.</param>
		public void SendWindowToBack(Window w)
		{
			if (w.IsActiveWindow)
			{
				w.IsActiveWindow = false;
			}
			for (int i = 0; i < Windows.Length; i++)
			{
				if (Windows[i] == w)
				{
					RemoveWindow(i);
					NeedToRedrawAll = true;

					Window[] winds = new Window[Windows.Length + 1];
					Array.Copy(Windows, winds, Windows.Length);
					winds[winds.Length - 1] = w;
					Windows = winds;
					Windows[0].IsActiveWindow = true;
					Taskbar.Modified = true;
					msgMan.SendDraw(w);
					return;
				}
			}
			throw new Exception("Unable to find the specified window.");
		}

		/// <summary>
		/// Bring the specified window to the front.
		/// </summary>
		/// <param name="w">The window to move to the front.</param>
		public void BringWindowToFront(Window w)
		{
			if (Windows.Length > 0)
			{
				Windows[0].IsActiveWindow = false;
			}
			w.IsActiveWindow = true;
			for (int i = 0; i < Windows.Length; i++)
			{
				if (Windows[i] == w)
				{
					RemoveWindow(i);
					Window wnd = Windows[0];
					InternalAddWindow(w);
					NeedToRedrawAll = true;
					Taskbar.Modified = true;
					msgMan.SendDraw(w);
					msgMan.SendDraw(wnd);
					return;
				}
			}
			throw new Exception("Specified Window not found!");
		}

		/// <summary>
		/// Removes the window at the specified index.
		/// </summary>
		/// <param name="indx">The index of the window to remove.</param>
		private void RemoveWindow(int indx)
		{
			lock (Windows)
			{
				AllWindows.Remove(Windows[indx].ID);
				Window[] tmp = new Window[Windows.Length - 1];
				Array.Copy(Windows, tmp, indx);
				Array.Copy(Windows, indx + 1, tmp, indx, Windows.Length - indx - 1);
				Windows = tmp;
				if (Windows.Length > 0)
				{
					Windows[0].IsActiveWindow = true;
				}
				NeedToRedrawAll = true;
			}
		}

		/// <summary>
		/// Removes the specified window.
		/// </summary>
		/// <param name="w">The window to remove.</param>
		private void RemoveWindow(Window w)
		{
			lock (Windows)
			{
				for (int i = 0; i < Windows.Length; i++)
				{
					if (Windows[i] == w)
					{
						RemoveWindow(i);
						return;
					}
				}
			}
		}

		/// <summary>
		/// Closes the specified window.
		/// </summary>
		/// <param name="w">The window to close.</param>
		public void CloseWindow(Window w)
		{
			lock (Windows)
			{
				for (int i = 0; i < Windows.Length; i++)
				{
					if (Windows[i] == w)
					{
						w.SendMessage(new Message(Message.MSG_EXIT, BitConverter.GetBytes((int)w.ID)));
						return;
					}
				}
			}
		}


		#region Handle Events

		#region Mouse Click
		/// <summary>
		/// Handles a MouseClick event.
		/// </summary>
		/// <param name="loc">The location of the mouse.</param>
		/// <param name="buttons">The MouseButtons that are pressed.</param>
		/// <param name="i">The image to draw to.</param>
		public void HandleMouseClick(Vec2 loc, MouseButtons buttons, Image i)
		{
			theMouse.MouseLocation = loc;


			byte[] msgDat = new byte[12];
			byte[] buf = BitConverter.GetBytes(loc.X);
			buf.CopyTo(msgDat, 0);
			buf = BitConverter.GetBytes(loc.Y);
			buf.CopyTo(msgDat, 4);
			buf = BitConverter.GetBytes((int)buttons);
			buf.CopyTo(msgDat, 8);
			foreach (Window w in Windows)
			{
				if (w.Bounds.IsInBounds(loc))
				{
					w.SendMessage(new Message(Message.MSG_CLICK, msgDat));
					break;
				}
			}


			if (Taskbar.Bounds.IsInBounds(loc))
			{
				Taskbar.DoClick(loc, buttons);
			}
			else
			{
				if (Taskbar.WasOverButton)
				{
					Taskbar.UndrawOverButton(Taskbar.WindowButtonBounds[Taskbar.overButtonIndx], Taskbar.Windows[Taskbar.overButtonIndx], Taskbar.overButtonIndx);
				}
			}
			this.Draw(i);
		}
		#endregion

		#region Mouse Move
		/// <summary>
		/// Processes a MouseMove event.
		/// </summary>
		/// <param name="loc">The location of the mouse.</param>
		/// <param name="buttons">The buttons of the mouse that are pressed.</param>
		/// <param name="i">The image to draw to.</param>
		public void HandleMouseMove(Vec2 loc, MouseButtons buttons, Image i)
		{
			theMouse.MouseLocation = loc;
			if (Taskbar.Bounds.IsInBounds(loc))
			{
				Taskbar.DoMouseMove(loc);
			}
			else
			{
				byte[] msgDat = new byte[12];
				byte[] buf = BitConverter.GetBytes(loc.X);
				buf.CopyTo(msgDat, 0);
				buf = BitConverter.GetBytes(loc.Y);
				buf.CopyTo(msgDat, 4);
				buf = BitConverter.GetBytes((int)buttons);
				buf.CopyTo(msgDat, 8);
				foreach (Window w in Windows)
				{
					w.SendMessage(new Message(Message.MSG_MOUSE_MOVE, msgDat));
				}

				if (Taskbar.WasOverButton)
				{
					Taskbar.UndrawOverButton(Taskbar.WindowButtonBounds[Taskbar.overButtonIndx], Taskbar.Windows[Taskbar.overButtonIndx], Taskbar.overButtonIndx);
				}
			}
			this.Draw(i);
		}
		#endregion

		#region Mouse Down
		/// <summary>
		/// Processes a MouseDown event.
		/// </summary>
		/// <param name="loc">The location of the mouse.</param>
		/// <param name="buttons">The MouseButtons that are pressed.</param>
		/// <param name="i">The Image to draw to.</param>
		public void HandleMouseDown(Vec2 loc, MouseButtons buttons, Image i)
		{
			theMouse.MouseLocation = loc;
			theMouse.PressButton(buttons);
			if (Windows.Length > 0)
			{
				if (ActiveWindow.Bounds.IsInBounds(loc))
				{
					byte[] msgDat = new byte[12];
					byte[] buf = BitConverter.GetBytes(loc.X);
					buf.CopyTo(msgDat, 0);
					buf = BitConverter.GetBytes(loc.Y);
					buf.CopyTo(msgDat, 4);
					buf = BitConverter.GetBytes((int)buttons);
					buf.CopyTo(msgDat, 8);
					ActiveWindow.SendMessage(new Message(Message.MSG_MOUSE_DOWN, msgDat));
					this.Draw(i);
				}
			}
		}
		#endregion

		#region Mouse Up
		/// <summary>
		/// Processes a MouseUp event.
		/// </summary>
		/// <param name="loc">The location of the mouse.</param>
		/// <param name="buttons">The MouseButtons that are still pressed.</param>
		/// <param name="i">The Image to draw to.</param>
		public void HandleMouseUp(Vec2 loc, MouseButtons buttons, Image i)
		{
			theMouse.MouseLocation = loc;
			theMouse.ReleaseButton(buttons);
			if (Windows.Length > 0)
			{
				byte[] msgDat = new byte[12];
				byte[] buf = BitConverter.GetBytes(loc.X);
				buf.CopyTo(msgDat, 0);
				buf = BitConverter.GetBytes(loc.Y);
				buf.CopyTo(msgDat, 4);
				buf = BitConverter.GetBytes((int)buttons);
				buf.CopyTo(msgDat, 8);
				ActiveWindow.SendMessage(new Message(Message.MSG_MOUSE_UP, msgDat));
				this.Draw(i);
			}
		}
		#endregion

		#endregion


	}
}
