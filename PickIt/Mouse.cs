using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using ExileCore.Shared;
using SharpDX;

namespace PickIt
{
    public class Mouse
    {
        public const int MOUSEEVENTF_MOVE = 0x0001;
        public const int MouseeventfLeftdown = 0x02;
        public const int MouseeventfLeftup = 0x04;
        public const int MouseeventfMiddown = 0x0020;
        public const int MouseeventfMidup = 0x0040;
        public const int MouseeventfRightdown = 0x0008;
        public const int MouseeventfRightup = 0x0010;
        public const int MouseEventWheel = 0x800;

        // 
        private const int MovementDelay = 10;
        private const int ClickDelay = 1;

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern bool BlockInput(bool fBlockIt);

        /// <summary>
        /// Sets the cursor position relative to the game window.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="gameWindow"></param>
        /// <returns></returns>
        public static bool SetCursorPos(int x, int y, RectangleF gameWindow)
        {
            return SetCursorPos(x + (int) gameWindow.X, y + (int) gameWindow.Y);
        }

        /// <summary>
        /// Sets the cursor position to the center of a given rectangle relative to the game window
        /// </summary>
        /// <param name="position"></param>
        /// <param name="gameWindow"></param>
        /// <returns></returns>
        public static bool SetCurosPosToCenterOfRec(RectangleF position, RectangleF gameWindow)
        {
            return SetCursorPos((int) (gameWindow.X + position.Center.X), (int) (gameWindow.Y + position.Center.Y));
        }

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point lpPoint);

        public static SharpDX.Point GetCursorPosition()
        {
            GetCursorPos(out var lpPoint);
            return lpPoint;
        }

        public static void LeftMouseDown()
        {
            mouse_event(MouseeventfLeftdown, 0, 0, 0, 0);
        }

        public static void LeftMouseUp()
        {
            mouse_event(MouseeventfLeftup, 0, 0, 0, 0);
        }

        public static void RightMouseDown()
        {
            mouse_event(MouseeventfRightdown, 0, 0, 0, 0);
        }

        public static void RightMouseUp()
        {
            mouse_event(MouseeventfRightup, 0, 0, 0, 0);
        }

        public static void SetCursorPosAndLeftClick(Vector2 coords, int extraDelay)
        {
            var posX = (int) coords.X;
            var posY = (int) coords.Y;
            SetCursorPos(posX, posY);
            Thread.Sleep(MovementDelay + extraDelay);
            mouse_event(MouseeventfLeftdown, 0, 0, 0, 0);
            Thread.Sleep(ClickDelay);
            mouse_event(MouseeventfLeftup, 0, 0, 0, 0);
        }

        public static void SetCursorPosAndLeftOrRightClick(Vector2 coords, int extraDelay, bool leftClick = true)
        {
            var posX = (int) coords.X;
            var posY = (int) coords.Y;
            SetCursorPos(posX, posY);
            Thread.Sleep(MovementDelay + extraDelay);

            if (leftClick)
                LeftClick(ClickDelay);
            else
                RightClick(ClickDelay);
        }

        public static void LeftClick(int extraDelay)
        {
            LeftMouseDown();
            if (extraDelay > 0) Thread.Sleep(ClickDelay);
            LeftMouseUp();
        }

        public static void RightClick(int extraDelay)
        {
            RightMouseDown();
            Thread.Sleep(ClickDelay);
            RightMouseUp();
        }

        public static void VerticalScroll(bool forward, int clicks)
        {
            if (forward)
                mouse_event(MouseEventWheel, 0, 0, clicks * 120, 0);
            else
                mouse_event(MouseEventWheel, 0, 0, -(clicks * 120), 0);
        }
        ////////////////////////////////////////////////////////////

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;

            public static implicit operator SharpDX.Point(Point point)
            {
                return new SharpDX.Point(point.X, point.Y);
            }
        }

        #region MyFix

        private static void SetCursorPosition(float x, float y)
        {
            SetCursorPos((int) x, (int) y);
        }

        public static Vector2 GetCursorPositionVector()
        {
            var currentMousePoint = GetCursorPosition();
            return new Vector2(currentMousePoint.X, currentMousePoint.Y);
        }

        public static void SetCursorPosition(Vector2 end)
        {
            var cursor = GetCursorPositionVector();
            var stepVector2 = new Vector2();
            var step = (float) Math.Sqrt(Vector2.Distance(cursor, end)) * 1.618f;
            if (step > 275) step = 240;
            stepVector2.X = (end.X - cursor.X) / step;
            stepVector2.Y = (end.Y - cursor.Y) / step;
            var fX = cursor.X;
            var fY = cursor.Y;

            for (var j = 0; j < step; j++)
            {
                fX += +stepVector2.X;
                fY += stepVector2.Y;
                SetCursorPosition(fX, fY);
                Thread.Sleep(2);
            }
        }

        public static void SetCursorPosAndLeftClickHuman(Vector2 coords, int extraDelay)
        {
            SetCursorPosition(coords);
            Thread.Sleep(MovementDelay + extraDelay);
            LeftMouseDown();
            Thread.Sleep(MovementDelay + extraDelay);
            LeftMouseUp();
        }

        public static void SetCursorPos(Vector2 vec)
        {
            SetCursorPos((int) vec.X, (int) vec.Y);
        }

        public static void MoveCursorToPosition(Vector2 vec)
        {
            SetCursorPos((int) vec.X, (int) vec.Y);
            MouseMove();
        }

        public static float speedMouse;

        public static IEnumerator SetCursorPosHuman(Vector2 vec)
        {
            var step = (float) Math.Sqrt(Vector2.Distance(GetCursorPositionVector(), vec)) * speedMouse / 20;

            if (step > 6)
            {
                for (var i = 0; i < step; i++)
                {
                    var vector2 = Vector2.SmoothStep(GetCursorPositionVector(), vec, i / step);
                    SetCursorPos((int) vector2.X, (int) vector2.Y);
                    yield return new WaitTime(1);
                }
            }
            else
                SetCursorPos(vec);
        }

        public static IEnumerator LeftClick()
        {
            LeftMouseDown();
            yield return new WaitTime(2);
            LeftMouseUp();
        }

        public static void MouseMove()
        {
            mouse_event(MOUSEEVENTF_MOVE, 0, 0, 0, 0);
        }

        #endregion
    }
}
