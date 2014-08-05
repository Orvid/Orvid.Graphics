using System;
using Orvid.Graphics;
using OForms.Controls;
using System.Collections.Generic;
using OForms.Mouse;
using OForms.Messaging;

namespace OForms.Windows
{
    /// <summary>
    /// The class that represents a single window.
    /// </summary>
    public class Window : IDisposable, IBufferedDrawable, IThread, IMessagable
    {
		/// <summary>
		/// The message queue.
		/// </summary>
		private LinkedQueue<Message> MessageQueue = new LinkedQueue<Message>();

		/// <summary>
		/// The function of this method
		/// is as described in <see cref="IMessagable"/>.
		/// </summary>
		public void SendMessage(Message msg)
		{
			MessageQueue.Enqueue(msg);
		}

		/// <summary>
		/// This method is as descibed in
		/// IThread.
		/// </summary>
		public int Run(string[] args)
		{
			Message msg;
			while (true)
			{
				if (MessageQueue.Count > 0)
				{
					msg = MessageQueue.Dequeue();
					switch (msg.MessageType)
					{
						case Message.MSG_EXIT:
							this.Close();
							Parent.SendMessage(new Message(Message.MSG_EXIT, msg.Data));
							return 0;
						case Message.MSG_PING:
							Parent.SendMessage(new Message(Message.MSG_PONG, msg.Data));
							break;
						case Message.MSG_DRAW:
							this.Draw();
							Parent.SendMessage(new Message(Message.MSG_DRAW, msg.Data));
							break;
						case Message.MSG_CLICK:
						{
							Vec2 loc = Vec2.Zero;
							MouseButtons buttons = MouseButtons.None;
							loc.X = BitConverter.ToInt32(msg.Data, 0);
							loc.Y = BitConverter.ToInt32(msg.Data, 4);
							buttons = (MouseButtons)BitConverter.ToInt32(msg.Data, 8);

							if (this.Bounds.IsInBounds(loc))
							{
								this.DoClick(loc, buttons);
							}
							break;
						}
						case Message.MSG_MOUSE_MOVE:
						{
							Vec2 loc = Vec2.Zero;
							MouseButtons buttons = MouseButtons.None;
							loc.X = BitConverter.ToInt32(msg.Data, 0);
							loc.Y = BitConverter.ToInt32(msg.Data, 4);
							buttons = (MouseButtons)BitConverter.ToInt32(msg.Data, 8);

							if (
								this.Bounds.IsInBounds(loc)
							||	this.IsDragging
							||	this.IsResizing
							||	this.isActiveWindow
								)
							{
								this.DoMouseMove(loc, buttons);
							}
							break;
						}
						case Message.MSG_MOUSE_DOWN:
						{
							Vec2 loc = Vec2.Zero;
							MouseButtons buttons = MouseButtons.None;
							loc.X = BitConverter.ToInt32(msg.Data, 0);
							loc.Y = BitConverter.ToInt32(msg.Data, 4);
							buttons = (MouseButtons)BitConverter.ToInt32(msg.Data, 8);

							this.DoMouseDown(loc, buttons);
							break;
						}
						case Message.MSG_MOUSE_UP:
						{
							Vec2 loc = Vec2.Zero;
							MouseButtons buttons = MouseButtons.None;
							loc.X = BitConverter.ToInt32(msg.Data, 0);
							loc.Y = BitConverter.ToInt32(msg.Data, 4);
							buttons = (MouseButtons)BitConverter.ToInt32(msg.Data, 8);

							this.DoMouseUp(loc, buttons);
							break;
						}
					}
				}
				else
				{
					System.Threading.Thread.Sleep(10);
				}
			}
		}


        /// <summary>
        /// The name of the window.
        /// </summary>
        public string Name;
        /// <summary>
        /// The parent WindowManager of this window.
        /// </summary>
        public WindowManager Parent;
        /// <summary>
        /// The location of the window. Use this internally only
        /// if you are reading the location. If you are changing 
        /// the location, use the public Location property instead, 
        /// so that the bounds, get properly reset.
        /// </summary>
        private Vec2 iLocation;
        /// <summary>
        /// The location of the window.
        /// </summary>
        public Vec2 Location
        {
            get
            {
                return iLocation;
            }
            set
            {
                Parent.NeedToRedrawAll = true;
                iLocation = value;
                ComputeBounds();
                // Doesn't change the physical size,
                // so buffers don't get reset.
            }
        }
        /// <summary>
        /// The minimum allowable window size in the X direction.
        /// </summary>
        private const int MinXWindowSize = 50;
        /// <summary>
        /// The minimum allowable window size in the Y direction.
        /// </summary>
        private const int MinYWindowSize = 40;
        /// <summary>
        /// The size of the window. Use this internally only
        /// if you are reading the size. If you are changing 
        /// the size, use the public Size property instead, 
        /// so that the bounds, and buffers get properly reset.
        /// </summary>
        private Vec2 iSize;
        /// <summary>
        /// The size of the window.
        /// </summary>
        public Vec2 Size
        {
            get
            {
                return iSize;
            }
            set
            {
                Parent.NeedToRedrawAll = true;
                if (value.X < MinXWindowSize)
                {
                    value.X = MinXWindowSize;
                }
                if (value.Y < MinYWindowSize)
                {
                    value.Y = MinYWindowSize;
                }
                iSize = value;
                ComputeBounds();
                ResetBuffers();
            }
        }
        /// <summary>
        /// The Bounds of this window.
        /// </summary>
        public BoundingBox Bounds;
        /// <summary>
        /// The BoundingBox for the window's contents.
        /// </summary>
        private BoundingBox ContentBounds;
        /// <summary>
        /// The BoundingBox for the header of the window.
        /// </summary>
        private BoundingBox HeaderBounds;
        /// <summary>
        /// The BoundingBox for the Close button.
        /// </summary>
        private BoundingBox CloseButtonBounds;
        /// <summary>
        /// The BoundingBox for the Maximize/Restore button.
        /// </summary>
        private BoundingBox MaxButtonBounds;
        /// <summary>
        /// The BoundingBox for the Minimize button.
        /// </summary>
        private BoundingBox MinButtonBounds;

		#region Window Border Bounds
		private BoundingBox WindowBorder_TopLeft;
		private BoundingBox WindowBorder_Top;
		private BoundingBox WindowBorder_TopRight;
		private BoundingBox WindowBorder_Right;
		private BoundingBox WindowBorder_BottomRight;
		private BoundingBox WindowBorder_Bottom;
		private BoundingBox WindowBorder_BottomLeft;
		private BoundingBox WindowBorder_Left;
		#endregion

		/// <summary>
        /// The height of the header. (in pixels)
        /// </summary>
        private const int HeaderHeight = 15;
        /// <summary>
        /// The thickness of the window border. (in pixels)
        /// </summary>
        private const int WindowBorderSize = 1;
        /// <summary>
        /// The overall buffer for the window.
        /// </summary>
        private Image WindowBuffer;
		/// <summary>
		/// The buffer as described by IBufferedDrawable.
		/// </summary>
		public Image Buffer
		{
			get { return WindowBuffer; }
		}
        /// <summary>
        /// The buffer for the content of the window.
        /// </summary>
        private Image ContentBuffer;
        /// <summary>
        /// The buffer for the header of the window.
        /// </summary>
        private Image HeaderBuffer;
        /// <summary>
        /// True if this is the currently active window.
        /// </summary>
        private bool isActiveWindow = false;  
        /// <summary>
        /// True if this is the currently active window.
        /// </summary>
        public bool IsActiveWindow
        {
            get
            {
                return isActiveWindow;
            }
            set
            {
                if (value != isActiveWindow)
                {
                    if (value)
                    {
                        FadingIn = true;
                    }
                    isActiveWindow = value;
                }
            }
        }
        /// <summary>
        /// True if the window is in the process of being dragged.
        /// </summary>
        private bool IsDragging = false;
        /// <summary>
        /// True if the window should fade in when selected.
        /// </summary>
        public bool ShouldFadeIn = true;
        /// <summary>
        /// True if we are fading in.
        /// </summary>
        private bool FadingIn = false;
        /// <summary>
        /// The current state of the window. 
        /// Don't use this field, use CurrentState
        /// instead.
        /// </summary>
        private WindowState iCurrentState = WindowState.Normal;
        /// <summary>
        /// The current WindowState of the window.
        /// </summary>
        public WindowState CurrentState
        {
            get
            {
                return iCurrentState;
            }
            set
            {
                if (iCurrentState != value)
                {
                    if (iCurrentState == WindowState.Maximized)
                    {
                        if (value == WindowState.Minimized)
                        {
                            WasMaximized = true;
                            iCurrentState = value;
                        }
                        else // It means window state is normal.
                        {
                            WasMaximized = false;
                            this.Location = PrevLoc;
                            this.Size = PrevSize;
                            iCurrentState = value;
                        }
                    }
                    // Should only be set to normal from the minimized state.
                    else if (iCurrentState == WindowState.Minimized)
					{
                        if (WasMaximized)
                        {
                            iCurrentState = WindowState.Maximized;
                        }
                        else
                        {
                            iCurrentState = value;
                        }
						this.Parent.BringWindowToFront(this);
                    }
                    else if (iCurrentState == WindowState.Normal)
                    {
                        if (value == WindowState.Maximized)
                        {
                            PrevLoc = iLocation;
                            this.Location = Vec2.Zero;
                            PrevSize = iSize;
                            this.Size = new Vec2(Parent.Size.X, Parent.Size.Y - WindowManager.TaskBarHeight) - 30;
                            iCurrentState = WindowState.Maximized;
                        }
                        else
                        {
                            iCurrentState = value;
                        }
                    }
                    else
                    {
                        throw new Exception("Unknown WindowState!");
                    }
                    ResetBuffers();
                    ComputeBounds();
                }
            }
        }
        /// <summary>
        /// Location of the window before it was maximized.
        /// </summary>
        private Vec2 PrevLoc;
        /// <summary>
        /// Size of the window before it was maximized.
        /// </summary>
        private Vec2 PrevSize;
        /// <summary>
        /// True if the window was Maximized when the window got Minimized.
        /// </summary>
        private bool WasMaximized = false;
        /// <summary>
        /// True if we're currently resizing.
        /// </summary>
        private bool IsResizing = false;
		/// <summary>
		/// The type of resize being performed.
		/// </summary>
		private ResizeType iResizeType = ResizeType.None;
        /// <summary>
        /// The size of the window when resizing started.
        /// </summary>
        private Vec2 InitSizeOnResizeStart;
        /// <summary>
        /// The location of the mouse when resizing started.
        /// </summary>
        private Vec2 InitResizeLocation;
        /// <summary>
        /// The location of the window when window dragging started.
        /// </summary>
        private Vec2 InitWindowLocOnDragStart;
        /// <summary>
        /// The location of the mouse when window dragging started.
        /// </summary>
		private Vec2 InitDraggingLocation;
		/// <summary>
		/// The size of a button on the header.
		/// </summary>
		private static Vec2 HeaderButtonSize = new Vec2(HeaderHeight - 3, HeaderHeight - 3);
        /// <summary>
        /// True if the last mouse move event had the mouse in
        /// the window header.
        /// </summary>
        private bool WasInHeader = false;
        /// <summary>
        /// True if the last mouse move event had the mouse
        /// over the Close button.
        /// </summary>
        private bool WasOverClose = false;
        /// <summary>
        /// True if the last mouse move event had the mouse
        /// over the Maximize/Restore button.
        /// </summary>
        private bool WasOverMax = false;
        /// <summary>
        /// True if the last mouse move event had the mouse
        /// over the Minimize button.
        /// </summary>
        private bool WasOverMin = false;
        /// <summary>
        /// The color to clear the ContentBuffer with when the window is inactive.
        /// </summary>
        public Pixel ClearInactiveColor = CustomColors.ControlLightLight;
        /// <summary>
        /// The color to clear the ContentBuffer with when the window is active.
        /// </summary>
		public Pixel ClearColor = CustomColors.ControlLightLight;
        /// <summary>
        /// True if the header has been drawn.
        /// </summary>
        private bool DrawnHeader = false;
        /// <summary>
        /// The controls in the window.
        /// </summary>
        public List<Control> Controls = new List<Control>();


        #region Colors
        /// <summary>
        /// The current background color of the Close button.
        /// </summary>
		private Pixel CurCloseButtonColor = DefaultCloseButtonColor;
        /// <summary>
        /// The current background color of the Maximize/Restore button.
        /// </summary>
		private Pixel CurMaxButtonColor = DefaultMaxButtonColor;
        /// <summary>
        /// The current background color of the Minimize button.
        /// </summary>
		private Pixel CurMinButtonColor = DefaultMinButtonColor;
		/// <summary>
		/// The color of the header text.
		/// </summary>
		public Pixel HeaderTextColor = CustomColors.ControlText;

        #region Default Colors
        /// <summary>
        /// The default background color of the Close button.
        /// </summary>
		private static Pixel DefaultCloseButtonColor = CustomColors.Control;
        /// <summary>
        /// The default background color of the Maximize/Restore button.
        /// </summary>
		private static Pixel DefaultMaxButtonColor = CustomColors.Control;
        /// <summary>
        /// The default background color of the Minimize button.
        /// </summary>
		private static Pixel DefaultMinButtonColor = CustomColors.Control;
        #endregion

        #endregion

		private int id;
		/// <summary>
		/// The ID of this window.
		/// </summary>
		public int ID { get { return id; } }

		/// <summary>
		/// The last window ID to
		/// be created.
		/// </summary>
		private static int lastID = 0;
		/// <summary>
		/// The lock used for ID
		/// setting.
		/// </summary>
		private static object IDLock = new object();
		
		/// <summary>
		/// Set's the window's ID.
		/// </summary>
		/// <param name="w">
		/// The window who's ID needs to be set.
		/// </param>
		internal static void SetWindowID(Window w)
		{
			lock (IDLock)
			{
				// Make the ID's get set
				// to a negative value if
				// we've used all the positive
				// ID's so far.
				if (lastID == int.MaxValue)
				{
					lastID = int.MinValue;
				}
				w.id = lastID;
				lastID++;
			}
		}

        /// <summary>
        /// The default constructor for a window.
        /// </summary>
        /// <param name="loc">The initial location of the window.</param>
        /// <param name="size">The size of the window.</param>
        /// <param name="name">The name of the window.</param>
        public Window(Vec2 loc, Vec2 size, string name)
        {
			SetWindowID(this);
            iLocation = loc;
            iSize = size;
            Name = name;
            ResetBuffers();
            ComputeBounds();
        }

        /// <summary>
        /// Gets a string representing this window.
        /// </summary>
        /// <returns>The window as a string.</returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Closes this window.
        /// </summary>
        public void Close()
        {
            Parent.CloseWindow(this);
        }

        /// <summary>
        /// Does the actual closing of this window.
        /// </summary>
        internal void DoClose()
        {
            foreach (Control c in Controls)
            {
                c.DoClosing();
            }
            this.Dispose();
        }

        /// <summary>
        /// Disposes of all of the resources used by this window.
        /// </summary>
        public void Dispose()
        {
            this.Bounds = null;
            this.CloseButtonBounds = null;
            this.ContentBounds = null;
            this.ContentBuffer = null;
            foreach (Control c in Controls)
            {
                c.DoBeforeDispose();
                c.Dispose();
                c.DoAfterDispose();
            }
            this.Controls = null;
            this.HeaderBounds = null;
            this.HeaderBuffer = null;
            this.MaxButtonBounds = null;
            this.MinButtonBounds = null;
            this.Parent = null;
            this.WindowBuffer = null;
            this.Name = null;
        }

        /// <summary>
        /// Resets all of the buffers, and redraws the HeaderBuffer.
        /// </summary>
        private void ResetBuffers()
        {
            WindowBuffer = new Image(iSize);
            WindowBuffer.Clear(Colors.BurlyWood);
            DrawnHeader = false;

            RedrawHeader(); // Draw the header.

            ContentBuffer = new Image(new Vec2(iSize.X - WindowBorderSize - WindowBorderSize, iSize.Y - HeaderHeight - WindowBorderSize));
        }

        /// <summary>
        /// Redraws the header.
        /// </summary>
        private void RedrawHeader()
        {
            HeaderBuffer = new Image(new Vec2(iSize.X, HeaderHeight));
			HeaderBuffer.DrawHGradientRectangle(
				Vec2.Zero, 
				new Vec2(iSize.X, HeaderHeight),
				CustomColors.ActiveCaption,
				CustomColors.GradientActiveCaption
			);
            DrawnHeader = false;

            HeaderBuffer.DrawString(
				new Vec2(3, 3), 
				Name, 
				WindowManager.WindowFont,
				10, 
				Orvid.Graphics.FontSupport.FontStyle.Normal,
				HeaderTextColor
			);

            RedrawCloseButton();
            RedrawMaxRestButton();
            RedrawMinButton();
        }

		#region Draw Close Button
		/// <summary>
		/// The image that represents the close button.
		/// </summary>
		private Image CloseButtonImage = new Image(HeaderButtonSize);
        /// <summary>
        /// Re-Draws the Close button on the HeaderBuffer.
        /// </summary>
        private void RedrawCloseButton()
        {
			CloseButtonImage.Clear(CurCloseButtonColor);
			CloseButtonImage.DrawRectangleOutline(Vec2.Zero, HeaderButtonSize - 1, CustomColors.ControlDarkDark);
			CloseButtonImage.DrawLine(new Vec2(2, 2), new Vec2(HeaderButtonSize.X - 3, HeaderButtonSize.Y - 3), CustomColors.ControlDarkDark);
			CloseButtonImage.DrawLine(new Vec2(HeaderButtonSize.X - 3, 2), new Vec2(2, HeaderButtonSize.Y - 3), CustomColors.ControlDarkDark);
			HeaderBuffer.DrawImage(new Vec2(iSize.X - HeaderHeight + 1, WindowBorderSize + 1), CloseButtonImage);
			DrawnHeader = false;
        }
        #endregion

		#region Draw Maximize/Restore Button
		/// <summary>
		/// The image that represents the 
		/// maximize or restore button.
		/// </summary>
		private Image MaxRestButtonImage = new Image(HeaderButtonSize);
        /// <summary>
        /// Re-Draws the Maximize/Restore button on the HeaderBuffer.
        /// </summary>
        private void RedrawMaxRestButton()
        {
            if (CurrentState == WindowState.Maximized) // Draw Restore Button.
            {
                #region Draw Restore Button
				MaxRestButtonImage.Clear(CurMaxButtonColor);
				MaxRestButtonImage.DrawRectangleOutline(Vec2.Zero, HeaderButtonSize - 1, CustomColors.ControlDarkDark);
                
				MaxRestButtonImage.DrawLines(new Vec2[] {
                    new Vec2(4, 2),
                    new Vec2(HeaderButtonSize.X - 3, 2),
                    new Vec2(HeaderButtonSize.X - 3, 3),
                    new Vec2(4, 3),
                    new Vec2(4, 5),
                    new Vec2(2, 5),
                    new Vec2(2, HeaderButtonSize.Y - 3),
                    new Vec2(HeaderButtonSize.X - 5, HeaderButtonSize.Y - 3),
                    new Vec2(HeaderButtonSize.X - 5, 5),
                    new Vec2(4, 5),
                    new Vec2(4, 6),
                    new Vec2(2, 6),
                    new Vec2(HeaderButtonSize.X - 5, 6),
                    new Vec2(HeaderButtonSize.X - 5, 7),
                    new Vec2(HeaderButtonSize.X - 3, 7),
                    new Vec2(HeaderButtonSize.X - 3, 2),
                }, CustomColors.ControlDarkDark);

				HeaderBuffer.DrawImage(new Vec2(iSize.X - ((HeaderButtonSize.X + 2) * 2), WindowBorderSize + 1), MaxRestButtonImage);
                #endregion
            }
            else // Draw Maximize Button.
            {
                #region Draw Maximize Button
				MaxRestButtonImage.Clear(CurMaxButtonColor);
				MaxRestButtonImage.DrawRectangleOutline(Vec2.Zero, HeaderButtonSize - 1, CustomColors.ControlDarkDark);
				MaxRestButtonImage.DrawLines(new Vec2[] {
                    new Vec2(2, 2),
                    new Vec2(HeaderButtonSize.X - 3, 2),
                    new Vec2(HeaderButtonSize.X - 3, HeaderButtonSize.Y - 3),
                    new Vec2(2, HeaderButtonSize.Y - 3),
                    new Vec2(2, 2),
                }, CustomColors.ControlDarkDark);
				MaxRestButtonImage.DrawLine(new Vec2(2, 3), new Vec2(HeaderButtonSize.X - 3, 3), CustomColors.ControlDarkDark);
				HeaderBuffer.DrawImage(new Vec2(iSize.X - ((HeaderButtonSize.X + 2) * 2), WindowBorderSize + 1), MaxRestButtonImage);
                #endregion
            }
            DrawnHeader = false;
        }
        #endregion

		#region Redraw Minimize Button
		/// <summary>
		/// The image that represents the minimize button.
		/// </summary>
		private Image MinimizeButtonImage = new Image(HeaderButtonSize);
        /// <summary>
        /// Re-Draws the Minimize button on the HeaderBuffer.
        /// </summary>
        private void RedrawMinButton()
        {
			MinimizeButtonImage.Clear(CurMinButtonColor);
			MinimizeButtonImage.DrawRectangleOutline(Vec2.Zero, HeaderButtonSize - 1, CustomColors.ControlDarkDark);

			MinimizeButtonImage.DrawRectangle(
				new Vec2(3, HeaderButtonSize.Y - 4),
				new Vec2(HeaderButtonSize.X - 3, HeaderButtonSize.Y - 2),
				CustomColors.ControlDarkDark);

			HeaderBuffer.DrawImage(new Vec2(iSize.X - ((HeaderButtonSize.X + 2) * 3), WindowBorderSize + 1), MinimizeButtonImage);

            DrawnHeader = false;
        }
        #endregion

        /// <summary>
        /// Re-Computes all of the bounding boxes.
        /// </summary>
        private void ComputeBounds()
        {
            this.Bounds = new BoundingBox(
                iLocation.X - 1,
                iLocation.X + iSize.X,
                iLocation.Y + iSize.Y,
                iLocation.Y - 1
            );
            this.HeaderBounds = new BoundingBox(
                iLocation.X + WindowBorderSize - 1,
				iLocation.X + iSize.X - WindowBorderSize,
                iLocation.Y + HeaderHeight,
				iLocation.Y + WindowBorderSize - 1
			);
            this.ContentBounds = new BoundingBox(
                iLocation.X + WindowBorderSize - 1,
                iLocation.X + (iSize.X - WindowBorderSize),
                iLocation.Y + (iSize.Y - WindowBorderSize),
                iLocation.Y + HeaderHeight - 1
            );

            this.CloseButtonBounds = new BoundingBox(

				iLocation.X + (iSize.X - HeaderHeight + 1),
                iLocation.X + (iSize.X - WindowBorderSize - 2),
                iLocation.Y + (HeaderHeight - 2),
                iLocation.Y + (WindowBorderSize + 1)
            );
            this.MaxButtonBounds = new BoundingBox(
				iLocation.X + (iSize.X - ((HeaderButtonSize.X + 2) * 2)),
                iLocation.X + (iSize.X - (HeaderButtonSize.X + 2 + WindowBorderSize + 2)),
                iLocation.Y + (HeaderHeight - 2),
                iLocation.Y + (WindowBorderSize + 1)
            );
            this.MinButtonBounds = new BoundingBox(
				iLocation.X + (iSize.X - ((HeaderButtonSize.X + 2) * 3)),
				iLocation.X + (iSize.X - (((HeaderButtonSize.X + 2) * 2) + WindowBorderSize + 2)),
                iLocation.Y + (HeaderHeight - 2),
                iLocation.Y + (WindowBorderSize + 1)
            );
			
			#region Window Border Bounds
			const int WindowBorderCornerSize = 10;
			this.WindowBorder_TopLeft = new BoundingBox(
				iLocation.X - 1,
				iLocation.X + WindowBorderCornerSize,
				iLocation.Y + WindowBorderCornerSize,
				iLocation.Y - 1
			);
			this.WindowBorder_Top = new BoundingBox(
				iLocation.X - 1,
				iLocation.X + iSize.X,
				iLocation.Y + WindowBorderCornerSize,
				iLocation.Y - 1
			);
			this.WindowBorder_TopRight = new BoundingBox(
				(iLocation.X + iSize.X) - WindowBorderCornerSize,
				iLocation.X + iSize.X,
				iLocation.Y + WindowBorderCornerSize,
				iLocation.Y - 1
			 );
			this.WindowBorder_Right = new BoundingBox(
				(iLocation.X + iSize.X) - WindowBorderCornerSize,
				iLocation.X + iSize.X,
				iLocation.Y + iSize.Y,
				iLocation.Y - 1
			 );
			this.WindowBorder_BottomRight = new BoundingBox(
				(iLocation.X + iSize.X) - WindowBorderCornerSize,
				iLocation.X + iSize.X,
				iLocation.Y + iSize.Y,
				(iLocation.Y + iSize.Y) - WindowBorderCornerSize
			 );
			this.WindowBorder_Bottom = new BoundingBox(
				iLocation.X - 1,
				iLocation.X + iSize.X,
				iLocation.Y + iSize.Y,
				(iLocation.Y + iSize.Y) - WindowBorderCornerSize
			 );
			this.WindowBorder_BottomLeft = new BoundingBox(
				iLocation.X - 1,
				iLocation.X + WindowBorderCornerSize,
				iLocation.Y + iSize.Y,
				(iLocation.Y + iSize.Y) - WindowBorderCornerSize
			 );
			this.WindowBorder_Left = new BoundingBox(
				 iLocation.X - 1,
				 iLocation.X + WindowBorderCornerSize,
				 iLocation.Y + iSize.Y,
				 iLocation.Y - 1
			 );
			#endregion

		}

        /// <summary>
        /// Draws this window.
        /// </summary>
        internal void Draw()
        {
            if (!DrawnHeader)
            {
                WindowBuffer.DrawImage(Vec2.Zero, HeaderBuffer);
				WindowBuffer.DrawRectangle(Vec2.Zero, new Vec2(WindowBorderSize, iSize.Y), CustomColors.WindowFrame);
				WindowBuffer.DrawRectangle(Vec2.Zero, new Vec2(iSize.X, WindowBorderSize), CustomColors.WindowFrame);
				WindowBuffer.DrawRectangle(new Vec2(iSize.X - WindowBorderSize, 0), iSize, CustomColors.WindowFrame);
                
                DrawnHeader = true;
            }

            if (IsActiveWindow)
            {
                if (FadingIn)
                {
                    ContentBuffer.Clear(new Pixel(ClearColor.R, ClearColor.G, ClearColor.B, 128));
                    FadingIn = false;
                }
                else
                {
                    ContentBuffer.Clear(ClearColor);
                }
            }
            else
            {
                ContentBuffer.Clear(ClearInactiveColor);
            }
            foreach (Control c in Controls)
            {
                c.Draw(ContentBuffer);
            }
            WindowBuffer.DrawImage(new Vec2(WindowBorderSize, HeaderHeight), ContentBuffer);
			WindowBuffer.DrawRectangle(new Vec2(0, iSize.Y - WindowBorderSize), new Vec2(iSize.X - 1, iSize.Y), CustomColors.WindowFrame);
			WindowBuffer.DrawLine(new Vec2(WindowBorderSize, HeaderHeight), new Vec2(iSize.X - WindowBorderSize - 1, HeaderHeight), CustomColors.WindowFrame);
			Parent.SendMessage(new Message(Message.MSG_DRAW, BitConverter.GetBytes(this.id)));
			
            //i.DrawImage(iLocation, WindowBuffer);

			//i.DrawBounds(this.CloseButtonBounds, Colors.LimeGreen);
			//i.DrawBounds(this.MaxButtonBounds, Colors.LimeGreen);
			//i.DrawBounds(this.MinButtonBounds, Colors.LimeGreen);

			//i.DrawBounds(ContentBounds, Colors.Goldenrod);

			//i.DrawBounds(HeaderBounds, Colors.MintyRose);

			//i.DrawBounds(this.Bounds, Colors.LimeGreen);

			//i.DrawBounds(WindowBorder_TopLeft, Colors.Goldenrod);
			//i.DrawBounds(WindowBorder_Top, Colors.HotPink);
			//i.DrawBounds(WindowBorder_TopRight, Colors.Goldenrod);
			//i.DrawBounds(WindowBorder_Right, Colors.HotPink);
			//i.DrawBounds(WindowBorder_BottomRight, Colors.Goldenrod);
			//i.DrawBounds(WindowBorder_Bottom, Colors.HotPink);
			//i.DrawBounds(WindowBorder_BottomLeft, Colors.Goldenrod);
			//i.DrawBounds(WindowBorder_Left, Colors.HotPink);
			
        }


        #region Do Events
        /// <summary>
        /// Processes a MouseClick event.
        /// </summary>
        /// <param name="loc">The location of the mouse.</param>
        /// <param name="button">The buttons that are pressed.</param>
        internal void DoClick(Vec2 loc, MouseButtons button)
        {
            if (IsActiveWindow)
            {
                if (ContentBounds.IsInBounds(loc))
                {
                    Vec2 RelativeLoc;
                    foreach (Control c in Controls)
                    {
                        RelativeLoc = new Vec2(loc.X - Location.X - WindowBorderSize, loc.Y - Location.Y - HeaderHeight);
                        if (c.Bounds.IsInBounds(RelativeLoc))
                        {
                            c.DoClick(RelativeLoc, button);
                        }
                    }
                }
                else
                {
                    if (HeaderBounds.IsInBounds(loc))
                    {
                        if (CloseButtonBounds.IsInBounds(loc))
                        {
                            this.Close();
                        }
                        else if (MaxButtonBounds.IsInBounds(loc))
                        {
                            if (CurrentState == WindowState.Maximized)
                            {
                                Parent.RestoreWindow(this);
                            }
                            else
                            {
                                Parent.MaximizeWindow(this);
                            }
                        }
                        else if (MinButtonBounds.IsInBounds(loc))
                        {
                            Parent.MinimizeWindow(this);
                        }
                    }
                    else // Window border was clicked, and we don't care about it.
                    {
                        //throw new Exception("Unknown part of the window clicked!");
                    }
                }
            }
            else
            {
                Parent.BringWindowToFront(this);
            }
        }

        /// <summary>
        /// Processes a MouseUp event.
        /// </summary>
        /// <param name="loc">The location of the mouse.</param>
        /// <param name="button">The buttons that are still pressed down.</param>
        internal void DoMouseUp(Vec2 loc, MouseButtons button)
        {
            if (IsDragging)
            {
                IsDragging = false;
                InitDraggingLocation = Vec2.Zero;
                InitWindowLocOnDragStart = Vec2.Zero;
            }
            else if (IsResizing)
            {
                IsResizing = false;
				Parent.Mouse.SetType(MouseType.Default);
                InitResizeLocation = Vec2.Zero;
                InitSizeOnResizeStart = Vec2.Zero;
            }
            else
            {
                Vec2 RelativeLoc;
                foreach (Control c in Controls)
                {
                    if (c.IsMouseDown)
                    {
                        RelativeLoc = new Vec2(loc.X - Location.X - WindowBorderSize, loc.Y - Location.Y - HeaderHeight);
                        c.IsMouseDown = false;
                        c.DoMouseUp(RelativeLoc, button);
                    }
                }
            }
        }

        /// <summary>
        /// Processes a MouseDown event.
        /// </summary>
        /// <param name="loc">The location of the mouse.</param>
        /// <param name="button">The buttons that are down.</param>
        internal void DoMouseDown(Vec2 loc, MouseButtons button)
        {
            if (ContentBounds.IsInBounds(loc))
            {
                Vec2 RelativeLoc;
                foreach (Control c in Controls)
                {
                    RelativeLoc = new Vec2(loc.X - Location.X - WindowBorderSize, loc.Y - Location.Y - HeaderHeight);
                    if (c.Bounds.IsInBounds(RelativeLoc))
                    {
                        c.IsMouseDown = true;
                        c.DoMouseDown(RelativeLoc, button);
                    }
                }
            }
            else
            {
                if (HeaderBounds.IsInBounds(loc) && this.CurrentState != WindowState.Maximized)
                {
                    IsDragging = true;
                    InitDraggingLocation = loc;
                    InitWindowLocOnDragStart = this.iLocation;
                }
                else 
				{
					// The border of the window was pressed,
					// check which border it was.

					// First check the 4 corners,
					// otherwise the side bounds
					// would trigger when they aren't
					// supposed to.
					if (WindowBorder_BottomRight.IsInBounds(loc))
					{
						Parent.Mouse.SetType(MouseType.DRResize);
						iResizeType = ResizeType.BottomRight;
					}
					else if (WindowBorder_TopRight.IsInBounds(loc))
					{
						Parent.Mouse.SetType(MouseType.DLResize);
						iResizeType = ResizeType.TopRight;
					}
					else if (WindowBorder_TopLeft.IsInBounds(loc))
					{
						Parent.Mouse.SetType(MouseType.DRResize);
						iResizeType = ResizeType.TopLeft;
					}
					else if (WindowBorder_BottomLeft.IsInBounds(loc))
					{
						Parent.Mouse.SetType(MouseType.DLResize);
						iResizeType = ResizeType.BottomLeft;
					}
					// Now for the left, right, bottom, and top.
					else if (WindowBorder_Right.IsInBounds(loc))
					{
						Parent.Mouse.SetType(MouseType.HResize);
						iResizeType = ResizeType.Right;
					}
					else if (WindowBorder_Bottom.IsInBounds(loc))
					{
						Parent.Mouse.SetType(MouseType.VResize);
						iResizeType = ResizeType.Bottom;
					}
					else if (WindowBorder_Left.IsInBounds(loc))
					{
						Parent.Mouse.SetType(MouseType.HResize);
						iResizeType = ResizeType.Left;
					}
					else if (WindowBorder_Top.IsInBounds(loc))
					{
						Parent.Mouse.SetType(MouseType.VResize);
						iResizeType = ResizeType.Top;
					}
					else
					{
						throw new Exception("Unknown Location Pressed!");
					}
                    IsResizing = true;
                    InitSizeOnResizeStart = this.iSize;
                    InitResizeLocation = loc;
					InitWindowLocOnDragStart = iLocation;
                }
            }
        }



        /// <summary>
        /// Checks if we were over buttons,
        /// and reset their colors if needed.
        /// </summary>
        private void CheckOldButtons()
        {
            if (WasOverClose)
            {
                CurCloseButtonColor = DefaultCloseButtonColor;
                WasOverClose = false;
                RedrawCloseButton();
            }
            else if (WasOverMax)
            {
                CurMaxButtonColor = DefaultMaxButtonColor;
                WasOverMax = false;
                RedrawMaxRestButton();
            }
            else if (WasOverMin)
            {
                CurMinButtonColor = DefaultMinButtonColor;
                WasOverMin = false;
                RedrawMinButton();
            }
        }

        /// <summary>
        /// Processes a MouseMove event.
        /// </summary>
        /// <param name="newLoc">The new location of the mouse.</param>
        /// <param name="button">The buttons on the mouse that are pressed.</param>
        internal void DoMouseMove(Vec2 newLoc, MouseButtons button)
		{
			Parent.Mouse.SetType(MouseType.Default);
            if (IsDragging)
            {
                Vec2 Transform = newLoc - InitDraggingLocation;
                this.Location = InitWindowLocOnDragStart + Transform;
            }
            else if (IsResizing)
            {
				int xBeforeSize;
				int yBeforeSize;
				Vec2 Transform;
				switch (iResizeType)
				{
					case ResizeType.TopLeft:
						Parent.Mouse.SetType(MouseType.DRResize);
						Transform = newLoc - InitResizeLocation;
						yBeforeSize = this.Size.Y;
						xBeforeSize = this.Size.X;
						this.Size = new Vec2(InitSizeOnResizeStart.X - Transform.X, InitSizeOnResizeStart.Y - Transform.Y);
						if (yBeforeSize != this.Size.Y)
						{
							this.Location = new Vec2(iLocation.X, InitWindowLocOnDragStart.Y + Transform.Y);
						}
						if (xBeforeSize != this.Size.X)
						{
							this.Location = new Vec2(InitWindowLocOnDragStart.X + Transform.X, iLocation.Y);
						}
						break;

					case ResizeType.TopRight:
						Parent.Mouse.SetType(MouseType.DLResize);
						Transform = newLoc - InitResizeLocation;
						yBeforeSize = this.Size.Y;
						this.Size = new Vec2(InitSizeOnResizeStart.X + Transform.X, InitSizeOnResizeStart.Y - Transform.Y);
						if (yBeforeSize != this.Size.Y)
						{
							this.Location = new Vec2(iLocation.X, InitWindowLocOnDragStart.Y + Transform.Y);
						}
						break;

					case ResizeType.BottomRight:
						Parent.Mouse.SetType(MouseType.DRResize);
						Transform = newLoc - InitResizeLocation;
						this.Size = InitSizeOnResizeStart + Transform;
						break;

					case ResizeType.BottomLeft:
						Parent.Mouse.SetType(MouseType.DLResize);
						Transform = newLoc - InitResizeLocation;
						xBeforeSize = this.Size.X;
						this.Size = new Vec2(InitSizeOnResizeStart.X - Transform.X, InitSizeOnResizeStart.Y + Transform.Y);
						if (xBeforeSize != this.Size.X)
						{
							this.Location = new Vec2(InitWindowLocOnDragStart.X + Transform.X, iLocation.Y);
						}
						break;

					case ResizeType.Top:
						Parent.Mouse.SetType(MouseType.VResize);
						Transform = newLoc - InitResizeLocation;
						yBeforeSize = this.Size.Y;
						this.Size = new Vec2(InitSizeOnResizeStart.X, InitSizeOnResizeStart.Y - Transform.Y);
						if (yBeforeSize != this.Size.Y)
						{
							this.Location = new Vec2(iLocation.X, InitWindowLocOnDragStart.Y + Transform.Y);
						}
						break;

					case ResizeType.Right:
						Parent.Mouse.SetType(MouseType.HResize);
						Transform = newLoc - InitResizeLocation;
						this.Size = new Vec2(InitSizeOnResizeStart.X + Transform.X, InitSizeOnResizeStart.Y);
						break;

					case ResizeType.Bottom:
						Parent.Mouse.SetType(MouseType.VResize);
						Transform = newLoc - InitResizeLocation;
						this.Size = new Vec2(InitSizeOnResizeStart.X, InitSizeOnResizeStart.Y + Transform.Y);
						break;

					case ResizeType.Left:
						Parent.Mouse.SetType(MouseType.HResize);
						Transform = newLoc - InitResizeLocation;
						xBeforeSize = this.Size.X;
						this.Size = new Vec2(InitSizeOnResizeStart.X - Transform.X, InitSizeOnResizeStart.Y);
						if (xBeforeSize != this.Size.X)
						{
							this.Location = new Vec2(InitWindowLocOnDragStart.X + Transform.X, iLocation.Y);
						}
						break;

					default:
						// The ResizeType.None isn't
						// in this statment for a reason.
						throw new Exception("Unknown Resize Type!");
				}
            }
            else
            {
                if (WasInHeader)
                {
                    if (HeaderBounds.IsInBounds(newLoc))
                    {
                        if (CloseButtonBounds.IsInBounds(newLoc))
                        {
                            if (!WasOverClose)
                            {
                                CheckOldButtons();
                                CurCloseButtonColor = Colors.Brown;
                                WasOverClose = true;
                                RedrawCloseButton();
                                return;
                            }
                            // Otherwise we've already done whats 
                            // needed for being over the close button.
                            return;
                        }
                        else if (MaxButtonBounds.IsInBounds(newLoc))
                        {
                            if (!WasOverMax)
                            {
                                CheckOldButtons();
                                CurMaxButtonColor = Colors.Brown;
                                WasOverMax = true;
                                RedrawMaxRestButton();
                                return;
                            }
                            // Otherwise we've already done whats 
                            // needed for being over the maximize/restore button.
                            return;
                        }
                        else if (MinButtonBounds.IsInBounds(newLoc))
                        {
                            if (!WasOverMin)
                            {
                                CheckOldButtons();
                                CurMinButtonColor = Colors.Brown;
                                WasOverMin = true;
                                RedrawMinButton();
                                return;
                            }
                            // Otherwise we've already done whats 
                            // needed for being over the minimize button.
                            return;
                        }
                        else
                        {
                            CheckOldButtons();
                        }
                    }
                    else // It's not in the header anymore.
                    {
                        CheckOldButtons();
                        WasInHeader = false;
                    }
                }
                if (ContentBounds.IsInBounds(newLoc))
                {
                    Vec2 RelativeLoc;
                    foreach (Control c in Controls)
                    {
                        RelativeLoc = new Vec2(newLoc.X - Location.X - WindowBorderSize, newLoc.Y - Location.Y - HeaderHeight);
                        if (!c.IsIn)
                        {
                            if (c.Bounds.IsInBounds(RelativeLoc))
                            {
                                c.IsIn = true;
                                c.DoMouseEnter(RelativeLoc, button);
                            }
                        }
                        else
                        {
                            if (!c.Bounds.IsInBounds(RelativeLoc))
                            {
                                c.IsIn = false;
                                c.DoMouseLeave(RelativeLoc, button);
                            }
                        }
                    }
                }
                else if (HeaderBounds.IsInBounds(newLoc))
                {
                    WasInHeader = true;
                    if (CloseButtonBounds.IsInBounds(newLoc))
                    {
                        CurCloseButtonColor = Colors.Brown;
                        WasOverClose = true;
                        RedrawCloseButton();
                    }
                    else if (MaxButtonBounds.IsInBounds(newLoc))
                    {
                        CurMaxButtonColor = Colors.Brown;
                        WasOverMax = true;
                        RedrawMaxRestButton();
                    }
                    else if (MinButtonBounds.IsInBounds(newLoc))
                    {
                        CurMinButtonColor = Colors.Brown;
                        WasOverMin = true;
                        RedrawMinButton();
                    }
                }
                else // the mouse was in the window border.
                {
					if (this.Bounds.IsInBounds(newLoc))
					{
						if (WindowBorder_BottomRight.IsInBounds(newLoc))
						{
							Parent.Mouse.SetType(MouseType.DRResize);
						}
						else if (WindowBorder_TopRight.IsInBounds(newLoc))
						{
							Parent.Mouse.SetType(MouseType.DLResize);
						}
						else if (WindowBorder_TopLeft.IsInBounds(newLoc))
						{
							Parent.Mouse.SetType(MouseType.DRResize);
						}
						else if (WindowBorder_BottomLeft.IsInBounds(newLoc))
						{
							Parent.Mouse.SetType(MouseType.DLResize);
						}
						// Now for the left, right, bottom, and top.
						else if (WindowBorder_Right.IsInBounds(newLoc))
						{
							Parent.Mouse.SetType(MouseType.HResize);
						}
						else if (WindowBorder_Bottom.IsInBounds(newLoc))
						{
							Parent.Mouse.SetType(MouseType.VResize);
						}
						else if (WindowBorder_Left.IsInBounds(newLoc))
						{
							Parent.Mouse.SetType(MouseType.HResize);
						}
						else if (WindowBorder_Top.IsInBounds(newLoc))
						{
							Parent.Mouse.SetType(MouseType.VResize);
						}
						else
						{
							throw new Exception("Unknown part of the window border!");
						}
					}
                }
            }
        }
        #endregion

        public static bool operator ==(Window w, Window w2)
        {
            if (w.id == w2.id)
            {
                return true;
            }
            return false;
        }

        public static bool operator !=(Window w, Window w2)
        {
            return ((w == w2) == false);
        }

        public override bool Equals(object obj)
        {
			if (obj is Window)
				return (this == (Window)obj);
			else
				return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
