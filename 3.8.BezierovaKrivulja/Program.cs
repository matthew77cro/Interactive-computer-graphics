using IRGLinearAlgebra;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;

namespace BezierovaKrivulja
{
    class Program
    {
        static void Main(string[] args)
        {

            new Window().Run();

        }
    }

    class Window : GameWindow
    {

        private const int POINT_GRAB_RADIUS = 10;

        private readonly List<Point> points = new List<Point>();

        private int movingPointIndex = -1;

        public Window()
            : base(640, 480, OpenTK.Graphics.GraphicsMode.Default, "LV3Z8")
        {

        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {

            if (e.Key == Key.Escape)
            {
                points.Clear();
                movingPointIndex = -1;
            }

        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {

            if (e.Button == MouseButton.Left)
            {
                points.Add(new Point(e.X, e.Y));
                Console.WriteLine(points.Count);
            }
            else if (e.Button == MouseButton.Right && movingPointIndex == -1)
            {
                int closestPointIndex = -1;
                double closestPointDistance = double.MaxValue;

                for (int i = 0; i < points.Count; i++)
                {
                    var point = points[i];
                    double distance = Math.Sqrt(Math.Pow(point.X-e.X,2) + Math.Pow(point.Y-e.Y,2));
                    if (distance >= closestPointDistance) continue;

                    closestPointIndex = i;
                    closestPointDistance = distance;
                }

                if (closestPointDistance < POINT_GRAB_RADIUS)
                    movingPointIndex = closestPointIndex;
            }

        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {

            if (movingPointIndex == -1)
                return;

            points[movingPointIndex].X = e.X;
            points[movingPointIndex].Y = e.Y;

        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Right)
                movingPointIndex = -1;
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

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(0.0f, 1.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.LoadIdentity();
            //crtanje scene :
            RenderScene();
            SwapBuffers();
        }

        private void RenderScene()
        {

            if (this.points.Count < 2)
                return;

            GL.PointSize(1.0f);
            GL.Color3(1.0f, 0.0f, 0.0f);

            var p = this.points.ToArray();
            
            GL.Begin(PrimitiveType.LineStrip);
            foreach (var point in p)
            {
                GL.Vertex2(point.X, point.Y);
            }
            GL.End();

            GL.Color3(0.0f, 0.0f, 1.0f);
            DrawBezierAprox(p, 100);
            GL.Color3(0.0f, 0.0f, 0.0f);
            DrawBezierInterpolation(p, 100);

        }

        IMatrix GetBernsteinCoefMatrix(int n)
        {
            int[] nCoef = ComputeFactors(n - 1);
            IMatrix m = new Matrix(n, n);

            for (int i = 0; i < n; i++)
            {
                int[] coef = ComputeFactors(n - i - 1);
                int outerSign = (n - i - 1) % 2 == 0 ? 1 : -1;

                for (int j = 0; j < n - i; j++)
                {

                    int sign = j % 2 == 0 ? 1 : -1;
                    m[j, i] = outerSign * sign * nCoef[i] * coef[j];

                }
            }

            return m;
        }
        IMatrix GetTMatrix(int n)
        {
            IMatrix m = new Matrix(n, n);

            for (int i = 0; i < n; i++)
            {
                double baseD = (double) i / (n - 1);

                for (int j = 0; j < n; j++)
                {

                    m[i, j] = Math.Pow(baseD, n - j - 1);

                }
            }

            return m;
        }

        int[] ComputeFactors(int n)
        {
            int[] factors = new int[n + 1];

            int i, a = 1;
            for (i = 1; i <= n + 1; i++)
            {
                factors[i - 1] = a;
                a = a * (n - i + 1) / i;
            }

            return factors;
        }

        void DrawBezierAprox(Point[] points, int divs)
        {
            Point p = new Point(0, 0);
            int n = points.Length - 1;
            int[] factors = ComputeFactors(n);
            double t, b;

            GL.Begin(PrimitiveType.LineStrip);
            for (int i = 0; i <= divs; i++)
            {
                t = 1.0 / divs * i;
                p.X = 0; p.Y = 0;

                for (int j = 0; j <= n; j++)
                {
                    if (j == 0)
                    {
                        b = factors[j] * Math.Pow(1 - t, n);
                    }
                    else if (j == n)
                    {
                        b = factors[j] * Math.Pow(t, n);
                    }
                    else
                    {
                        b = factors[j] * Math.Pow(t, j) * Math.Pow(1 - t, n - j);
                    }
                    p.X += b * points[j].X;
                    p.Y += b * points[j].Y;
                }
                GL.Vertex2(p.X, p.Y);
            }
            GL.End();
        }

        void DrawBezierInterpolation(Point[] points, int divs)
        {

            IMatrix b = GetBernsteinCoefMatrix(points.Length);
            IMatrix bInv = b.NInvert();

            IMatrix t = GetTMatrix(points.Length);
            IMatrix tInv = t.NInvert();

            IMatrix bInvtInv = bInv * tInv;

            IVector pX = new Vector(false, true, new double[points.Length]);
            IVector pY = new Vector(false, true, new double[points.Length]);
            for (int i = 0; i < points.Length; i++)
            {
                pX[i] = points[i].X;
                pY[i] = points[i].Y;
            }

            IVector rX = (bInvtInv * pX.ToColumnMatrix(false)).ToVector(false);
            IVector rY = (bInvtInv * pY.ToColumnMatrix(false)).ToVector(false);

            Point[] points2 = new Point[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points2[i] = new Point(rX[i], rY[i]);
            }

            DrawBezierAprox(points2, divs);
        }

    }

    class Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

}
