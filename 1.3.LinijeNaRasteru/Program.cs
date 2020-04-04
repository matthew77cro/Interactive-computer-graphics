using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;

namespace LinijeNaRasteru
{

    class Program
    {
        public static void Main(string[] args)
        {
            new Window(new WindowModel()).Run();
        }
    }

    class WindowModel
    {

        private readonly List<LineSegment> lines = new List<LineSegment>();

        public DrawingStage DrawStage { get; private set; }
        public Point P1 { get; private set; } // Temp point for drawing
        public int MouseX { get; private set; }
        public int MouseY { get; private set; }
        public int Size { get { return lines.Count; } }
        public bool Control { get; set; }
        public bool Cutting { get; set; }
        public Rectangle CuttingRectangle { get; set; }
        public int WindowWidth { get; private set; }
        public int WindowHeight { get; private set; }

        public WindowModel()
        {
            DrawStage = DrawingStage.NONE;
            MouseX = 0;
            MouseY = 0;
        }

        public void SetMouse(int x, int y)
        {
            MouseX = x;
            MouseY = y;
        }

        public void UserClicked()
        {
            switch (DrawStage)
            {
                case DrawingStage.NONE:
                    P1 = new Point(MouseX, MouseY);
                    DrawStage = DrawingStage.POINT;
                    break;
                case DrawingStage.POINT:
                    Point p2 = new Point(MouseX, MouseY);
                    LineSegment l = new LineSegment(P1, p2);
                    lines.Add(l);
                    DrawStage = DrawingStage.NONE;
                    break;
            }

        }

        public void WindowResized(int width, int height)
        {
            WindowWidth = width;
            WindowHeight = height;
            int w4 = width / 4;
            int h4 = height / 4;
            int w2 = width / 2;
            int h2 = height / 2;
            CuttingRectangle = new Rectangle(new Point(w4, h4 + h2 - 1), new Point(w4 + w2 - 1, h4 + h2 - 1), new Point(w4, h4), new Point(w4 + w2 - 1, h4));
        }

        public LineSegment this[int i]
        {
            get
            {
                return lines[i];
            }
            set
            {
                lines[i] = value;
            }
        }

        public int ConvertYCoord(int y)
        {
            return WindowHeight - y;
        }
    }

    class Window : GameWindow
    {

        private readonly WindowModel model;

        public Window(WindowModel model)
            : base(640, 480, OpenTK.Graphics.GraphicsMode.Default, "LV1Z3")
        {
            this.model = model;
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            model.SetMouse(e.X, model.ConvertYCoord(e.Y));
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case 'o':
                    model.Cutting = model.Cutting ? false : true;
                    break;
                case 'k':
                    model.Control = model.Control ? false : true;
                    break;
                default:
                    break;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button != MouseButton.Left)
                return;
            model.UserClicked();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.LoadIdentity();
            //crtanje scene :
            RenderScene();
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width - 1, Height - 1, 0, 0, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            model.WindowResized(Width, Height);
        }

        private void RenderScene()
        {
            GL.PointSize(1f);

            if (model.Cutting)
            {
                GL.Color3(0f, 1f, 0f);
                GL.Begin(PrimitiveType.LineLoop);
                GL.Vertex2(model.CuttingRectangle.UpperLeft.X, model.ConvertYCoord(model.CuttingRectangle.UpperLeft.Y));
                GL.Vertex2(model.CuttingRectangle.LowerLeft.X, model.ConvertYCoord(model.CuttingRectangle.LowerLeft.Y));
                GL.Vertex2(model.CuttingRectangle.LowerRight.X, model.ConvertYCoord(model.CuttingRectangle.LowerRight.Y));
                GL.Vertex2(model.CuttingRectangle.UpperRight.X, model.ConvertYCoord(model.CuttingRectangle.UpperRight.Y));
                GL.End();
            }

            GL.Color3(0f, 0f, 0f);
            for (int i = 0; i < model.Size; i++)
            {
                LineSegment l = model[i];
                DrawLine(l.Point1.X, l.Point1.Y, l.Point2.X, l.Point2.Y);
            }

            switch (model.DrawStage)
            {
                case DrawingStage.POINT:
                    DrawLine(model.P1.X, model.P1.Y, model.MouseX, model.MouseY);
                    break;
            }
        }

        private void DrawLine(int xs, int ys, int xe, int ye)
        {
            GL.Color3(0f, 0f, 0f);

            if (model.Cutting)
            {
                // Algoritam Cohen Sutherlanda
                DrawLineBresenhamCutting(xs, ys, xe, ye);
            }
            else
            {
                DrawLineBresenham(xs, ys, xe, ye);
            }

            if (model.Control)
            {
                double distance = Math.Sqrt(Math.Pow(xe - xs, 2) + Math.Pow(ye - ys, 2));
                int dx = (int)Math.Round(4 / distance * (ye - ys));
                int dy = (int)Math.Round(-4 / distance * (xe - xs));
                GL.Color3(1f, 0f, 0f);
                DrawLineBresenham(xs + dx, ys + dy, xe + dx, ye + dy);
            }
        }

        private byte PointCode(int x, int y)
        {
            byte code = 0;

            if (y > model.CuttingRectangle.UpperLeft.Y) code |= 8;
            else if (y < model.CuttingRectangle.LowerLeft.Y) code |= 4;

            if (x > model.CuttingRectangle.UpperRight.X) code |= 2;
            else if (x < model.CuttingRectangle.UpperLeft.X) code |= 1;

            return code;
        }

        private void DrawLineBresenhamCutting(int xs, int ys, int xe, int ye)
        {
            int newXs = xs, newYs = ys, newXe = xe, newYe = ye;

            byte startCode = PointCode(xs, ys);
            byte endCode = PointCode(xe, ye);

            if ((startCode & endCode) != 0) return;
            if (startCode == 0 && endCode == 0)
            {
                DrawLineBresenham(xs, ys, xe, ye);
                return;
            }

            if (newYs > model.CuttingRectangle.UpperLeft.Y)
            {
                newXs = (int)Math.Round(xs + (xe - xs) * (model.CuttingRectangle.UpperLeft.Y - ys) / ((double)ye - ys));
                newYs = model.CuttingRectangle.UpperLeft.Y;
            }
            else if (ys < model.CuttingRectangle.LowerLeft.Y)
            {
                newXs = (int)Math.Round(xs + (xe - xs) * (model.CuttingRectangle.LowerLeft.Y - ys) / ((double)ye - ys));
                newYs = model.CuttingRectangle.LowerLeft.Y;
            }

            if (newYe > model.CuttingRectangle.UpperLeft.Y)
            {
                newXe = (int)Math.Round(xs + (xe - xs) * (model.CuttingRectangle.UpperLeft.Y - ys) / ((double)ye - ys));
                newYe = model.CuttingRectangle.UpperLeft.Y;
            }
            else if (newYe < model.CuttingRectangle.LowerLeft.Y)
            {
                newXe = (int)Math.Round(xs + (xe - xs) * (model.CuttingRectangle.LowerLeft.Y - ys) / ((double)ye - ys));
                newYe = model.CuttingRectangle.LowerLeft.Y;
            }

            if (newXs > model.CuttingRectangle.UpperRight.X)
            {
                newYs = ys + (ye - ys) * (model.CuttingRectangle.UpperRight.X - xs) / (xe - xs);
                newXs = model.CuttingRectangle.UpperRight.X;
            }
            else if (newXs < model.CuttingRectangle.UpperLeft.X)
            {
                newYs = ys + (ye - ys) * (model.CuttingRectangle.UpperLeft.X - xs) / (xe - xs);
                newXs = model.CuttingRectangle.UpperLeft.X;
            }

            if (newXe > model.CuttingRectangle.UpperRight.X)
            {
                newYe = ys + (ye - ys) * (model.CuttingRectangle.UpperRight.X - xs) / (xe - xs);
                newXe = model.CuttingRectangle.UpperRight.X;
            }
            else if (newXe < model.CuttingRectangle.UpperLeft.X)
            {
                newYe = ys + (ye - ys) * (model.CuttingRectangle.UpperLeft.X - xs) / (xe - xs);
                newXe = model.CuttingRectangle.UpperLeft.X;
            }

            DrawLineBresenham(newXs, newYs, newXe, newYe);
        }

        private void DrawLineBresenham(int xs, int ys, int xe, int ye)
        {
            if (xs <= xe)
            {
                if (ys <= ye)
                {
                    DrawLineBresenham2(xs, ys, xe, ye);
                }
                else
                {
                    DrawLineBresenham3(xs, ys, xe, ye);
                }
            }
            else
            {
                if (ys >= ye)
                {
                    DrawLineBresenham2(xe, ye, xs, ys);
                }
                else
                {
                    DrawLineBresenham3(xe, ye, xs, ys);
                }
            }
        }

        private void DrawLineBresenham2(int xs, int ys, int xe, int ye)
        {
            if (ye - ys <= xe - xs)
            {
                int a = 2 * (ye - ys);
                int yc = ys;
                int yf = -(xe - xs);
                int korekcija = -2 * (xe - xs);

                GL.Begin(PrimitiveType.Points);
                for (int x = xs; x <= xe; x++)
                {
                    GL.Vertex2(x, model.ConvertYCoord(yc));
                    yf += a;
                    if (yf >= 0)
                    {
                        yf += korekcija;
                        yc++;
                    }
                }
                GL.End();
            }
            else
            {
                int tmp = xe; xe = ye; ye = tmp;
                tmp = xs; xs = ys; ys = tmp;

                int a = 2 * (ye - ys);
                int yc = ys;
                int yf = -(xe - xs);
                int korekcija = -2 * (xe - xs);

                GL.Begin(PrimitiveType.Points);
                for (int x = xs; x <= xe; x++)
                {
                    GL.Vertex2(yc, model.ConvertYCoord(x));
                    yf += a;
                    if (yf >= 0)
                    {
                        yf += korekcija;
                        yc++;
                    }
                }
                GL.End();
            }
        }

        private void DrawLineBresenham3(int xs, int ys, int xe, int ye)
        {
            if (-(ye - ys) <= xe - xs)
            {
                int a = 2 * (ye - ys);
                int yc = ys;
                int yf = xe - xs;
                int korekcija = 2 * (xe - xs);

                GL.Begin(PrimitiveType.Points);
                for (int x = xs; x <= xe; x++)
                {
                    GL.Vertex2(x, model.ConvertYCoord(yc));
                    yf += a;
                    if (yf <= 0)
                    {
                        yf += korekcija;
                        yc--;
                    }
                }
                GL.End();
            }
            else
            {
                int tmp = xe; xe = ys; ys = tmp;
                tmp = xs; xs = ye; ye = tmp;

                int a = 2 * (ye - ys);
                int yc = ys;
                int yf = xe - xs;
                int korekcija = 2 * (xe - xs);

                GL.Begin(PrimitiveType.Points);
                for (int x = xs; x <= xe; x++)
                {
                    GL.Vertex2(yc, model.ConvertYCoord(x));
                    yf += a;
                    if (yf <= 0)
                    {
                        yf += korekcija;
                        yc--;
                    }
                }
                GL.End();
            }
        }
    }

    class Point
    {
        public int X { get; }
        public int Y { get; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    class LineSegment
    {
        public Point Point1 { get; }
        public Point Point2 { get; }

        public LineSegment(Point p1, Point p2)
        {
            Point1 = p1;
            Point2 = p2;
        }
    }

    class Rectangle
    {
        public Point UpperLeft { get; }
        public Point UpperRight { get; }
        public Point LowerLeft { get; }
        public Point LowerRight { get; }

        public Rectangle(Point upperLeft, Point upperRight, Point lowerLeft, Point lowerRight)
        {
            UpperLeft = upperLeft ?? throw new ArgumentNullException(nameof(upperLeft));
            UpperRight = upperRight ?? throw new ArgumentNullException(nameof(upperRight));
            LowerLeft = lowerLeft ?? throw new ArgumentNullException(nameof(lowerLeft));
            LowerRight = lowerRight ?? throw new ArgumentNullException(nameof(lowerRight));
        }
    }

    enum DrawingStage : byte
    {
        NONE,
        POINT
    }

}
