using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;

namespace PrviOpenGL
{

    class Z1
    {
        public static void Main(string[] args)
        {
            new Window(new WindowModel()).Run();
        }
    }

    class WindowModel
    {
        private static readonly List<Color> colors = new List<Color>()
        {
            new Color(1, 0, 0),
            new Color(0, 1, 0),
            new Color(0, 0, 1),
            new Color(0, 1, 1),
            new Color(1, 1, 0),
            new Color(1, 0, 1)
        };

        private int currentColorIndex = 0;
        private readonly List<Triangle> triangles = new List<Triangle>();

        public DrawingStage DrawStage { get; private set; }
        public Point P1 { get; private set; }
        public Point P2 { get; private set; } // Temp points for drawing
        public int MouseX { get; private set; }
        public int MouseY { get; private set; }
        public int Size { get { return triangles.Count; } }

        public WindowModel()
        {
            DrawStage = DrawingStage.NONE;
            MouseX = 0;
            MouseY = 0;
        }

        public void ColorNext()
        {
            if (currentColorIndex == colors.Count - 1) currentColorIndex = 0;
            else currentColorIndex++;
        }

        public void ColorPrev()
        {
            if (currentColorIndex == 0) currentColorIndex = colors.Count - 1;
            else currentColorIndex--;
        }

        public Color GetColor()
        {
            return colors[currentColorIndex];
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
                    DrawStage = DrawingStage.POINT1;
                    break;
                case DrawingStage.POINT1:
                    P2 = new Point(MouseX, MouseY);
                    DrawStage = DrawingStage.POINT2;
                    break;
                case DrawingStage.POINT2:
                    Point p3 = new Point(MouseX, MouseY);
                    Triangle t = new Triangle(P1, P2, p3, colors[currentColorIndex]);
                    triangles.Add(t);
                    DrawStage = DrawingStage.NONE;
                    break;
            }

        }

        public Triangle this[int i]
        {
            get
            {
                return triangles[i];
            }
            set
            {
                triangles[i] = value;
            }
        }
    }

    class Window : GameWindow
    {

        private readonly WindowModel model;

        public Window(WindowModel model)
            : base(640, 480, OpenTK.Graphics.GraphicsMode.Default, "LV1Z2")
        {
            this.model = model;
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            model.SetMouse(e.X, e.Y);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            switch(e.KeyChar)
            {
                case 'n':
                    model.ColorNext();
                    break;
                case 'p':
                    model.ColorPrev();
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
        }

        private void RenderScene()
        {
            GL.PointSize(1.0f);
            Color currentColor = model.GetColor();
            GL.Color3(currentColor.R, currentColor.G, currentColor.B);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(Width - 15, 0);
            GL.Vertex2(Width - 15, 15);
            GL.Vertex2(Width, 15);
            GL.Vertex2(Width, 0);
            GL.End();

            for(int i = 0; i < model.Size; i++)
            {
                Triangle t = model[i];
                GL.Color3(t.TriangleColor.R, t.TriangleColor.G, t.TriangleColor.B);
                GL.Begin(PrimitiveType.Triangles);
                GL.Vertex2(t.Point1.X, t.Point1.Y);
                GL.Vertex2(t.Point2.X, t.Point2.Y);
                GL.Vertex2(t.Point3.X, t.Point3.Y);
                GL.End();
            }

            GL.Color3(currentColor.R, currentColor.G, currentColor.B);
            switch (model.DrawStage)
            {
                case DrawingStage.POINT1:
                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex2(model.P1.X, model.P1.Y);
                    GL.Vertex2(model.MouseX, model.MouseY);
                    GL.End();
                    break;
                case DrawingStage.POINT2:
                    GL.Begin(PrimitiveType.Triangles);
                    GL.Vertex2(model.P1.X, model.P1.Y);
                    GL.Vertex2(model.P2.X, model.P2.Y);
                    GL.Vertex2(model.MouseX, model.MouseY);
                    GL.End();
                    break;
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

    class Triangle
    {
        public Point Point1 { get; }
        public Point Point2 { get; }
        public Point Point3 { get; }
        public Color TriangleColor { get; }

        public Triangle(Point p1, Point p2, Point p3, Color color)
        {
            Point1 = p1;
            Point2 = p2;
            Point3 = p3;
            TriangleColor = color;
        }
    }

    enum DrawingStage : byte
    {
        NONE,
        POINT1,
        POINT2
    }

    class Color
    {
        public float R { get; }
        public float G { get; }
        public float B { get; }

        public Color(float r, float g, float b)
        {
            this.R = r;
            this.G = g;
            this.B = b;
        }
    }

}
