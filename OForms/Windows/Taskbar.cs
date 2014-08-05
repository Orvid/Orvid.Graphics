﻿using System;
using Orvid.Graphics;
using OForms.Mouse;
using OForms.Controls;

namespace OForms.Windows
{
    /// <summary>
    /// The class that represents the Taskbar.
    /// </summary>
    internal class Taskbar
    {
        /// <summary>
        /// The internal buffer of the taskbar.
        /// </summary>
        private Image Buffer;
        /// <summary>
        /// All of the windows in the taskbar.
        /// </summary>
        public Window[] Windows;
        /// <summary>
        /// The parent window manager of the taskbar.
        /// </summary>
        private WindowManager Manager;
        /// <summary>
        /// The location of the taskbar on the window manager.
        /// </summary>
        private Vec2 TaskbarLocation;
        /// <summary>
        /// The bounds of the taskbar.
        /// </summary>
        public BoundingBox Bounds;
        /// <summary>
        /// The color to clear the back of the taskbar with.
        /// </summary>
        public Pixel TaskbarClearColor = CustomColors.Control;
        /// <summary>
        /// The color to outline the taskbar with.
        /// </summary>
        public Pixel TaskbarOutlineColor = Colors.Crimson;
        /// <summary>
        /// The color to draw an inactive window's back in.
        /// </summary>
        public Pixel WindowInactiveBackColor = Colors.Brown;
        /// <summary>
        /// The color to draw an inactive window's outline in.
        /// </summary>
        public Pixel WindowInactiveLineColor = Colors.Blue;
        /// <summary>
        /// The color to draw the active window's back in.
        /// </summary>
        public Pixel WindowActiveBackColor = Colors.Green;
        /// <summary>
        /// The color to draw the active window's outline in.
        /// </summary>
        public Pixel WindowActiveLineColor = Colors.Black;
        /// <summary>
        /// The color to draw the back of an over active window's back in.
        /// </summary>
        public Pixel WindowActiveOverBackColor = Colors.CadetBlue;
        /// <summary>
        /// The color to draw the back of an over inactive window's back in.
        /// </summary>
        public Pixel WindowInactiveOverBackColor = Colors.Chocolate;
        /// <summary>
        /// The color to draw the text on the taskbar in.
        /// </summary>
        public Pixel TaskbarTextColor = Colors.Black;
        /// <summary>
        /// The default width of a button for a window.
        /// </summary>
        private const int WindowButtonWidth = 160;
        /// <summary>
        /// The margin around a window button.
        /// </summary>
        private const int WindowButtonMargin = 1;
        /// <summary>
        /// The height of the taskbar.
        /// </summary>
        public const int TaskBarHeight = 20;
        /// <summary>
        /// The maximum width for the text on a window button.
        /// </summary>
        public const int MaxTextWidth = WindowButtonWidth - 6;
        /// <summary>
        /// The bounds of all of the window buttons.
        /// </summary>
        internal BoundingBox[] WindowButtonBounds;
        /// <summary>
        /// The index of the button that the mouse is over.
        /// </summary>
        internal int overButtonIndx = 0;
        /// <summary>
        /// True if the over button has been drawn.
        /// </summary>
        private bool DrawnOverButton = false;
        /// <summary>
        /// True if the taskbar has been modified since
		/// it was last drawn.
        /// </summary>
        internal bool Modified = true;
        /// <summary>
        /// True if the mouse was over a window button.
        /// </summary>
        internal bool WasOverButton = false;


        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="mangr">The parent window manager.</param>
        public Taskbar(WindowManager mangr)
        {
            this.Windows = new Window[0];
            this.Manager = mangr;
            this.Bounds = new BoundingBox(0, mangr.Size.X, mangr.Size.Y, mangr.Size.Y - TaskBarHeight);
            this.Buffer = new Image(mangr.Size.X, WindowManager.TaskBarHeight + 1);
            this.Buffer.Clear(TaskbarClearColor);
            this.WindowButtonBounds = new BoundingBox[0];
            this.TaskbarLocation = new Vec2(0, Manager.Size.Y - TaskBarHeight);
        }

        /// <summary>
        /// Adds the specified window to the taskbar.
        /// </summary>
        /// <param name="w">The window to add.</param>
        public void AddWindow(Window w)
        {
            Window[] tmp = new Window[Windows.Length + 1];
            Array.Copy(Windows, tmp, Windows.Length);
            tmp[tmp.Length - 1] = w;
            Windows = tmp;
            Modified = true;
        }

        /// <summary>
        /// Removes the specified window from the taskbar.
        /// </summary>
        /// <param name="w">The window to remove.</param>
        public void RemoveWindow(Window w)
        {
            uint i;
            for (i = 0; i < Windows.Length; i++)
            {
                if (Windows[i] == w)
                {
                    break;
                }
            }
            Window[] tmp = new Window[Windows.Length - 1];
            Array.Copy(Windows, tmp, i);
            Array.Copy(Windows, i + 1, tmp, i, Windows.Length - i - 1);
            Windows = tmp;
            Modified = true;
        }

		private Button[] TaskbarButtons;

        /// <summary>
        /// Redraws the Buffer.
        /// </summary>
        private void RedrawBuffer()
        {
            WindowButtonBounds = new BoundingBox[Windows.Length];
			TaskbarButtons = new Button[Windows.Length];
            Buffer.Clear(TaskbarClearColor);
            int loc = 20;
            if (Windows.Length * (WindowButtonWidth + (2 * WindowButtonMargin)) < Manager.Size.X - 20)
            {
                #region No Dynamic Size
                Vec2 tl;
                Vec2 tr;
                Vec2 br;
                Vec2 bl;
                Window w;
                for (uint ind = 0; ind < Windows.Length; ind++)
                {
                    w = Windows[ind];

                    tl = new Vec2(
                        loc + WindowButtonMargin,
                        (TaskBarHeight - (TaskBarHeight - 2 - WindowButtonMargin))
                    );
                    tr = new Vec2(
                        loc + WindowButtonMargin + WindowButtonWidth,
                        (TaskBarHeight - (TaskBarHeight - 2 - WindowButtonMargin))
                    );
                    br = new Vec2(
                        loc + WindowButtonMargin + WindowButtonWidth,
                        (TaskBarHeight - 2 - WindowButtonMargin)
                    );
                    bl = new Vec2(
                        loc + WindowButtonMargin,
                        (TaskBarHeight - 2 - WindowButtonMargin)
                    );

					Button btn = new Button(tl, br - tl);
					btn.Text = w.Name;
					if (w.IsActiveWindow)
						btn.DoMouseDown(Vec2.Zero, MouseButtons.None);
					else
						btn.DoMouseLeave(Vec2.Zero, MouseButtons.None);
					btn.Draw(Buffer);

                    WindowButtonBounds[ind] = new BoundingBox(
                        loc + WindowButtonMargin,
                        loc + WindowButtonMargin + WindowButtonWidth,
                        Manager.Size.Y - (TaskBarHeight - (TaskBarHeight - 2 - WindowButtonMargin)),
                        Manager.Size.Y - (TaskBarHeight - 2 - WindowButtonMargin)
                    );
					TaskbarButtons[ind] = btn;

                    loc += WindowButtonMargin + WindowButtonMargin + WindowButtonWidth;
                }
                #endregion
            }
            else
            {
                #region Dynamic Size
                uint len = (uint)Manager.Size.X - 20;
                int ButtonWidth = (int)Math.Floor((double)((len / Windows.Length) - 2));
                if (ButtonWidth > 5)
                {
                    Vec2 tl;
                    Vec2 tr;
                    Vec2 br;
                    Vec2 bl;
                    Window w;
                    for (uint ind = 0; ind < Windows.Length; ind++)
                    {
                        w = Windows[ind];

                        tl = new Vec2(
                            loc + WindowButtonMargin,
                            (TaskBarHeight - (TaskBarHeight - 2 - WindowButtonMargin))
                        );
                        tr = new Vec2(
                            loc + WindowButtonMargin + ButtonWidth,
                            (TaskBarHeight - (TaskBarHeight - 2 - WindowButtonMargin))
                        );
                        br = new Vec2(
                            loc + WindowButtonMargin + ButtonWidth,
                            (TaskBarHeight - 2 - WindowButtonMargin)
                        );
                        bl = new Vec2(
                            loc + WindowButtonMargin,
                            (TaskBarHeight - 2 - WindowButtonMargin)
                        );

                        WindowButtonBounds[ind] = new BoundingBox(
                            loc + WindowButtonMargin,
                            loc + WindowButtonMargin + ButtonWidth,
                            Manager.Size.Y - (TaskBarHeight - (TaskBarHeight - 2 - WindowButtonMargin)),
                            Manager.Size.Y - (TaskBarHeight - 2 - WindowButtonMargin)
                        );

                        if (w.IsActiveWindow && w.CurrentState != WindowState.Minimized)
                        {
                            Buffer.DrawRectangle(tl, br, WindowActiveBackColor);
                            Buffer.DrawLines(new Vec2[] { tl, tr, br, bl, tl }, WindowActiveLineColor);
                        }
                        else
                        {
                            Buffer.DrawRectangle(tl, br, WindowInactiveBackColor);
                            Buffer.DrawLines(new Vec2[] { tl, tr, br, bl, tl }, WindowInactiveLineColor);
                        }

                        //DrawWindowName(WindowButtonBounds[ind], w);

                        loc += WindowButtonMargin + WindowButtonMargin + ButtonWidth;
                    }
                }
                else
                {
                    Buffer.DrawRectangle(new Vec2(24, 4), new Vec2(Buffer.Width - 10, Buffer.Height - 4), Colors.Cyan);
                }
                #endregion
            }
            Modified = false;
        }

        /// <summary>
        /// Draws the taskbar on the specified image.
        /// </summary>
        /// <param name="i">The image to draw on.</param>
        public void Draw(Image i)
        {
			EnsureDrawn();
            i.DrawImage(TaskbarLocation, Buffer);
        }

        /// <summary>
        /// Undraws the over WindowButton.
        /// </summary>
        /// <param name="bounds">Bounds of the window to undraw.</param>
        /// <param name="w">The window to undraw.</param>
        internal void UndrawOverButton(BoundingBox bounds, Window w, int winIndex)
        {
            if (w.IsActiveWindow && w.CurrentState != WindowState.Minimized)
            {
				TaskbarButtons[winIndex].DoMouseDown(Vec2.Zero, MouseButtons.None);
            }
            else
			{
				TaskbarButtons[winIndex].DoMouseLeave(Vec2.Zero, MouseButtons.None);
            }

			TaskbarButtons[winIndex].Draw(Buffer);
            WasOverButton = false;
        }

        /// <summary>
        /// Draws the over WindowButton.
        /// </summary>
        /// <param name="bounds">The bounds of the window to draw.</param>
        /// <param name="w">The window to draw.</param>
		private void DrawOverButton(BoundingBox bounds, Window w, int winIndex)
        {
            if (w.IsActiveWindow && w.CurrentState != WindowState.Minimized)
            {
				TaskbarButtons[winIndex].DoMouseDown(Vec2.Zero, MouseButtons.None);
            }
            else
			{
				TaskbarButtons[winIndex].DoMouseLeave(Vec2.Zero, MouseButtons.None);
            }

			TaskbarButtons[winIndex].Draw(Buffer);
            
			WasOverButton = true;
        }

		/// <summary>
		/// Redraws the buffer
		/// if it's been modified.
		/// </summary>
		private void EnsureDrawn()
		{
			if (Modified)
			{
				RedrawBuffer();
			}
		}

        /// <summary>
        /// Processes the mouse move event.
        /// </summary>
        /// <param name="loc">Location of the mouse.</param>
        internal void DoMouseMove(Vec2 loc)
		{
			EnsureDrawn();
            if (!WasOverButton)
            {
                for (int i = 0; i < Windows.Length; i++)
                {
                    if (WindowButtonBounds[i].IsInBounds(loc))
                    {
                        DrawOverButton(WindowButtonBounds[i], Windows[i], i);
                        overButtonIndx = i;
                        return;
                    }
                }
            }
            else
            {
                if (!WindowButtonBounds[overButtonIndx].IsInBounds(loc) || !DrawnOverButton)
                {
                    UndrawOverButton(WindowButtonBounds[overButtonIndx], Windows[overButtonIndx], overButtonIndx);
                    for (int i = 0; i < Windows.Length; i++)
                    {
                        if (WindowButtonBounds[i].IsInBounds(loc))
                        {
                            DrawOverButton(WindowButtonBounds[i], Windows[i], i);
                            overButtonIndx = i;
                            return;
                        }
                    }
                    overButtonIndx = 0;
                    DrawnOverButton = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Processes a click event.
        /// </summary>
        /// <param name="loc">The location of the mouse.</param>
        /// <param name="buttons">The buttons that are pressed.</param>
        internal void DoClick(Vec2 loc, MouseButtons buttons)
		{
			EnsureDrawn();
            if (WasOverButton)
            {
                if (WindowButtonBounds[overButtonIndx].IsInBounds(loc))
				{
					
                    if (Windows[overButtonIndx].IsActiveWindow)
                    {
                        Manager.MinimizeWindow(Windows[overButtonIndx]);
                    }
                    else if (Windows[overButtonIndx].CurrentState == WindowState.Minimized)
                    {
                        Manager.RestoreWindow(Windows[overButtonIndx]);
                    }
                    else
                    {
                        Manager.BringWindowToFront(Windows[overButtonIndx]);
                    }
                    DrawnOverButton = false;
                    DoMouseMove(loc);
                }
                else
                {
                    for (int i = 0; i < Windows.Length; i++)
                    {
                        if (WindowButtonBounds[i].IsInBounds(loc))
                        {
                            DrawOverButton(WindowButtonBounds[i], Windows[i], i);
                            overButtonIndx = i;
                            DrawnOverButton = false;
                            if (Windows[i].IsActiveWindow)
                            {
                                Manager.MinimizeWindow(Windows[i]);
                            }
                            else if (Windows[i].CurrentState == WindowState.Minimized)
                            {
                                Manager.RestoreWindow(Windows[i]);
                            }
                            else
                            {
                                Manager.BringWindowToFront(Windows[i]);
                            }
                            DoMouseMove(loc);
                            return;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < Windows.Length; i++)
                {
                    if (WindowButtonBounds[i].IsInBounds(loc))
                    {
                        DrawOverButton(WindowButtonBounds[i], Windows[i], i);
                        overButtonIndx = i;
                        DrawnOverButton = false;
                        if (Windows[i].IsActiveWindow)
                        {
                            Manager.MinimizeWindow(Windows[i]);
                        }
                        else if (Windows[i].CurrentState == WindowState.Minimized)
                        {
                            Manager.RestoreWindow(Windows[i]);
                        }
                        else
                        {
                            Manager.BringWindowToFront(Windows[i]);
                        }
                        DoMouseMove(loc);
                        return;
                    }
                }
            }
        }
    }
}
